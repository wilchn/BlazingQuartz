using System;
using BlazingQuartz.Core.Models;
using BlazingQuartz.Jobs;
using Quartz;
using Quartz.Impl.Matchers;

namespace BlazingQuartz.Core.Services
{
    public class SchedulerService : ISchedulerService
    {
        private readonly ISchedulerFactory _schedulerFactory;

        public SchedulerService(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
        }

        public async IAsyncEnumerable<ScheduleModel> GetAllJobsAsync()
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobGroupNames = await scheduler.GetJobGroupNames();

            foreach (var jobGrp in jobGroupNames)
            {
                var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(jobGrp));

                foreach (var jobKey in jobKeys)
                {
                    await foreach (var job in GetScheduleModelsAsync(jobKey))
                    {
                        yield return job;
                    }
                }
            }
        }

        public async Task<ScheduleModel> GetScheduleModelAsync(ITrigger trigger)
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            var jobDetail = await scheduler.GetJobDetail(trigger.JobKey);

            return CreateScheduleModel(jobDetail, trigger);
        }

        private async IAsyncEnumerable<ScheduleModel> GetScheduleModelsAsync(JobKey jobkey, TriggerKey? triggerKey = null)
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            var jobDetail = await scheduler.GetJobDetail(jobkey);
            var jobTriggers = await scheduler.GetTriggersOfJob(jobkey);

            if (!jobTriggers.Any())
            {
                yield return new ScheduleModel
                {
                    JobName = jobkey.Name,
                    JobGroup = jobkey.Group,
                    JobType = jobDetail?.JobType.ToString(),
                    JobStatus = JobStatus.NoTrigger
                };
            }
            else
            {
                foreach (var trigger in jobTriggers)
                {
                    yield return CreateScheduleModel(jobDetail, trigger);
                }
            }
        }

        public ScheduleModel CreateScheduleModel(IJobDetail? jobDetail, ITrigger trigger)
        {
            return new ScheduleModel
            {
                JobName = jobDetail?.Key.Name,
                JobGroup = jobDetail?.Key.Group ?? "No Group",
                JobType = jobDetail?.JobType.ToString(),
                JobDescription = jobDetail?.Description,
                TriggerName = trigger.Key.Name,
                TriggerGroup = trigger.Key.Group,
                TriggerDescription = trigger.Description,
                TriggerType = trigger.GetTriggerType(),
                TriggerTypeClassName = trigger.GetType().Name,
                NextTriggerTime = trigger.GetNextFireTimeUtc(),
                PreviousTriggerTime = trigger.GetPreviousFireTimeUtc(),
                JobStatus = JobStatus.Idle
            };
        }

        static int count = 0;
        public async Task CreateSchedule(ScheduleModel model)
        {
            // define the job and tie it to our HelloJob class
            IJobDetail job = JobBuilder.Create<HelloJob>()
                .WithIdentity("myJob" + count++, "group" + Random.Shared.Next(500)) // name "myJob", group "group1"
                .WithDescription("This is generated job")
                .Build();

            // Trigger the job to run now, and then every 40 seconds
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("myTrigger" + count, "group1" + Random.Shared.Next(500))
                .StartNow()
                .WithCalendarIntervalSchedule(x => x
                    .WithIntervalInDays(1))
                // .WithDailyTimeIntervalSchedule(x => x
                //     .WithIntervalInSeconds(5)
                //     .WithIntervalInMinutes(1)

                //     .StartingDailyAt(new TimeOfDay(10, 16, 0))
                //     .EndingDailyAt(new TimeOfDay(10, 17, 0))
                //     )
                // .WithSimpleSchedule(x => x
                //     .WithIntervalInSeconds(40)
                //     .RepeatForever())
                .Build();

            var scheduler = await _schedulerFactory.GetScheduler();
            // Tell quartz to schedule the job using our trigger
            await scheduler.ScheduleJob(job, trigger);
        }
    }
}

