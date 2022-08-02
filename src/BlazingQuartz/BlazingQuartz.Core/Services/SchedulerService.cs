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

        public async Task<IReadOnlyCollection<string>> GetJobGroups()
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            return await scheduler.GetJobGroupNames();
        }

        public async Task<IReadOnlyCollection<string>> GetTriggerGroups()
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            return await scheduler.GetTriggerGroupNames();
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

        public async Task CreateSchedule(JobDetailModel jobDetailModel, TriggerDetailModel triggerDetailModel)
        {
            ArgumentNullException.ThrowIfNull(jobDetailModel.JobClass);

            var job = JobBuilder.Create(jobDetailModel.JobClass)
                .WithIdentity(jobDetailModel.Name, jobDetailModel.Group)
                .WithDescription(jobDetailModel.Description)
                .UsingJobData(new JobDataMap(jobDetailModel.JobDataMap))
                .Build();

            var tbldr = TriggerBuilder.Create()
                .WithIdentity(triggerDetailModel.Name, triggerDetailModel.Group)
                .WithDescription(triggerDetailModel.Description)
                .WithPriority(triggerDetailModel.Priority)
                .ModifiedByCalendar(triggerDetailModel.ModifiedByCalendar);

            if (triggerDetailModel.StartDate.HasValue)
            {
                DateTimeOffset startTime;

                if (triggerDetailModel.StartTimeSpan.HasValue)
                {
                    var dt = triggerDetailModel.StartDate.Value.Add(triggerDetailModel.StartTimeSpan.Value);
                    startTime = new DateTimeOffset(dt,
                        triggerDetailModel.StartTimezone.BaseUtcOffset);
                }
                else
                {
                    startTime = new DateTimeOffset(triggerDetailModel.StartDate.Value,
                        triggerDetailModel.StartTimezone.BaseUtcOffset);
                }

                tbldr = tbldr.StartAt(startTime);
            }
            else
            {
                tbldr = tbldr.StartNow();
            }

            if (triggerDetailModel.EndDate.HasValue)
            {
                DateTimeOffset endTime;

                if (triggerDetailModel.EndTimeSpan.HasValue)
                {
                    var dt = triggerDetailModel.EndDate.Value.Add(triggerDetailModel.EndTimeSpan.Value);
                    endTime = new DateTimeOffset(dt,
                        triggerDetailModel.StartTimezone.BaseUtcOffset);
                }
                else
                {
                    endTime = new DateTimeOffset(triggerDetailModel.EndDate.Value,
                        triggerDetailModel.StartTimezone.BaseUtcOffset);
                }

                tbldr = tbldr.EndAt(endTime);
            }

            switch (triggerDetailModel.TriggerType)
            {
                case TriggerType.Cron:
                    ArgumentNullException.ThrowIfNull(triggerDetailModel.CronExpression);
                    tbldr = tbldr.WithCronSchedule(triggerDetailModel.CronExpression,
                        x =>
                        {
                            switch(triggerDetailModel.MisfireAction)
                            {
                                case MisfireAction.DoNothing:
                                    x.WithMisfireHandlingInstructionDoNothing();
                                    break;
                                case MisfireAction.FireOnceNow:
                                    x.WithMisfireHandlingInstructionFireAndProceed();
                                    break;
                                case MisfireAction.IgnoreMisfirePolicy:
                                    x.WithMisfireHandlingInstructionIgnoreMisfires();
                                    break;
                            }
                            x.InTimeZone(triggerDetailModel.InTimeZone);
                        });
                    break;
                case TriggerType.Daily:
                    tbldr = tbldr.WithDailyTimeIntervalSchedule(x =>
                    {
                        switch (triggerDetailModel.MisfireAction)
                        {
                            case MisfireAction.DoNothing:
                                x.WithMisfireHandlingInstructionDoNothing();
                                break;
                            case MisfireAction.FireOnceNow:
                                x.WithMisfireHandlingInstructionFireAndProceed();
                                break;
                            case MisfireAction.IgnoreMisfirePolicy:
                                x.WithMisfireHandlingInstructionIgnoreMisfires();
                                break;
                        }
                        x.OnDaysOfTheWeek(triggerDetailModel.GetDailyOnDaysOfWeek());
                        if (triggerDetailModel.StartDailyTime.HasValue)
                        {
                            x.StartingDailyAt(triggerDetailModel.StartDailyTime.Value.ToTimeOfDay());
                        }
                        if (triggerDetailModel.EndDailyTime.HasValue)
                        {
                            x.EndingDailyAt(triggerDetailModel.EndDailyTime.Value.ToTimeOfDay());
                        }
                        x.InTimeZone(triggerDetailModel.InTimeZone);
                        if (triggerDetailModel.TriggerInterval > 0 && triggerDetailModel.TriggerIntervalUnit.HasValue)
                        {
                            x.WithInterval(triggerDetailModel.TriggerInterval,
                                triggerDetailModel.TriggerIntervalUnit.Value.ToQuartzIntervalUnit());
                        }
                        x.WithRepeatCount(triggerDetailModel.RepeatCount);
                    });
                    break;
                case TriggerType.Simple:
                    tbldr = tbldr.WithSimpleSchedule(x =>
                    {
                        switch (triggerDetailModel.MisfireAction)
                        {
                            case MisfireAction.FireNow:
                                x.WithMisfireHandlingInstructionFireNow();
                                break;
                            case MisfireAction.RescheduleNextWithExistingCount:
                                x.WithMisfireHandlingInstructionNextWithExistingCount();
                                break;
                            case MisfireAction.RescheduleNextWithRemainingCount:
                                x.WithMisfireHandlingInstructionNextWithRemainingCount();
                                break;
                            case MisfireAction.RescheduleNowWithExistingRepeatCount:
                                x.WithMisfireHandlingInstructionNowWithExistingCount();
                                break;
                            case MisfireAction.RescheduleNowWithRemainingRepeatCount:
                                x.WithMisfireHandlingInstructionNowWithRemainingCount();
                                break;
                            case MisfireAction.IgnoreMisfirePolicy:
                                x.WithMisfireHandlingInstructionIgnoreMisfires();
                                break;
                        }

                        if (triggerDetailModel.TriggerInterval > 0 && triggerDetailModel.TriggerIntervalUnit.HasValue)
                        {
                            TimeSpan timeSpan;
                            switch(triggerDetailModel.TriggerIntervalUnit.Value)
                            {
                                case IntervalUnit.Millisecond:
                                    timeSpan = TimeSpan.FromMilliseconds(triggerDetailModel.TriggerInterval);
                                    break;
                                case IntervalUnit.Second:
                                    timeSpan = TimeSpan.FromSeconds(triggerDetailModel.TriggerInterval);
                                    break;
                                case IntervalUnit.Minute:
                                    timeSpan = TimeSpan.FromSeconds(triggerDetailModel.TriggerInterval);
                                    break;
                                case IntervalUnit.Hour:
                                    timeSpan = TimeSpan.FromHours(triggerDetailModel.TriggerInterval);
                                    break;
                                case IntervalUnit.Day:
                                    timeSpan = TimeSpan.FromDays(triggerDetailModel.TriggerInterval);
                                    break;
                                default:
                                    throw new NotSupportedException(
                                        $"Interval unit {triggerDetailModel.TriggerIntervalUnit} is not supported for SimpleTrigger.");
                            }
                            x.WithInterval(timeSpan);
                        }

                        if (triggerDetailModel.RepeatForever)
                            x.RepeatForever();
                        else
                            x.WithRepeatCount(triggerDetailModel.RepeatCount);
                    });
                    break;
                case TriggerType.Calendar:
                    tbldr = tbldr.WithCalendarIntervalSchedule(x =>
                    {
                        switch (triggerDetailModel.MisfireAction)
                        {
                            case MisfireAction.DoNothing:
                                x.WithMisfireHandlingInstructionDoNothing();
                                break;
                            case MisfireAction.FireOnceNow:
                                x.WithMisfireHandlingInstructionFireAndProceed();
                                break;
                            case MisfireAction.IgnoreMisfirePolicy:
                                x.WithMisfireHandlingInstructionIgnoreMisfires();
                                break;
                        }

                        x.InTimeZone(triggerDetailModel.InTimeZone); // not implemented in UI. confusing with start timezone
                        if (triggerDetailModel.TriggerInterval > 0 && triggerDetailModel.TriggerIntervalUnit.HasValue)
                        {
                            x.WithInterval(triggerDetailModel.TriggerInterval,
                                triggerDetailModel.TriggerIntervalUnit.Value.ToQuartzIntervalUnit());
                        }
                    });
                    break;
            }

            var trigger = tbldr.Build();

            var scheduler = await _schedulerFactory.GetScheduler();
            await scheduler.ScheduleJob(job, trigger);
        }

        public async Task<JobDetailModel?> GetJobDetail(string jobName, string groupName)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jd = await scheduler.GetJobDetail(new JobKey(jobName, groupName));

            if (jd == null)
                return null;

            return new JobDetailModel
            {
                Name = jd.Key.Name,
                Group = jd.Key.Group,
                Description = jd.Description,
                JobDataMap = jd.JobDataMap,
                JobClass = jd.JobType
            };
        }

        public async Task<TriggerDetailModel?> GetTriggerDetail(string triggerName, string triggerGroup)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var trigger = await scheduler.GetTrigger(new TriggerKey(triggerName, triggerGroup));
            
            if (trigger == null)
                return null;

            var triggerType = trigger.GetTriggerType();


            var model = new TriggerDetailModel
            {
                Name = trigger.Key.Name,
                Group = trigger.Key.Group,
                Description = trigger.Description,
                TriggerDataMap = trigger.JobDataMap,
                EndDate = trigger.EndTimeUtc?.Date,
                EndTimeSpan = trigger.EndTimeUtc?.TimeOfDay,
                StartDate = trigger.StartTimeUtc.Date,
                StartTimeSpan = trigger.StartTimeUtc.TimeOfDay,
                StartTimezone = TimeZoneInfo.Utc,
                TriggerType = triggerType,
                ModifiedByCalendar = trigger.CalendarName,
                Priority = trigger.Priority,
                
            };

            switch(trigger.MisfireInstruction)
            {
                case MisfireInstruction.IgnoreMisfirePolicy:
                    model.MisfireAction = MisfireAction.IgnoreMisfirePolicy;
                    break;
                // comment out same as SmartPolicy
                //case MisfireInstruction.InstructionNotSet:
                //    model.MisfireAction = MisfireAction.InstructionNotSet;
                //    break;
                case MisfireInstruction.SmartPolicy:
                    model.MisfireAction = MisfireAction.SmartPolicy;
                    break;
            }

            switch (triggerType)
            {
                case TriggerType.Cron:
                    var cron = (ICronTrigger)trigger;
                    model.CronExpression = cron.CronExpressionString;
                    model.InTimeZone = cron.TimeZone;
                    switch (cron.MisfireInstruction)
                    {
                        case MisfireInstruction.CronTrigger.DoNothing:
                            model.MisfireAction = MisfireAction.DoNothing;
                            break;
                        case MisfireInstruction.CronTrigger.FireOnceNow:
                            model.MisfireAction = MisfireAction.FireOnceNow;
                            break;
                    }
                    break;
                case TriggerType.Daily:
                    var daily = (IDailyTimeIntervalTrigger)trigger;
                    foreach (var dow in daily.DaysOfWeek)
                    {
                        model.DailyDayOfWeek[(int)dow] = true;
                    }
                    switch (daily.MisfireInstruction)
                    {
                        case MisfireInstruction.DailyTimeIntervalTrigger.DoNothing:
                            model.MisfireAction = MisfireAction.DoNothing;
                            break;
                        case MisfireInstruction.DailyTimeIntervalTrigger.FireOnceNow:
                            model.MisfireAction = MisfireAction.FireOnceNow;
                            break;
                    }
                    model.RepeatCount = daily.RepeatCount;
                    model.TriggerInterval = daily.RepeatInterval;
                    model.TriggerIntervalUnit = daily.RepeatIntervalUnit.ToBlazingQuartzIntervalUnit();
                    model.InTimeZone = daily.TimeZone;
                    model.StartDailyTime = new TimeSpan(daily.StartTimeOfDay.Hour, daily.StartTimeOfDay.Minute, daily.StartTimeOfDay.Second);
                    model.EndDailyTime = new TimeSpan(daily.EndTimeOfDay.Hour, daily.EndTimeOfDay.Minute, daily.EndTimeOfDay.Second);
                    break;
                case TriggerType.Simple:
                    var simple = (ISimpleTrigger)trigger;
                    model = PopulateSimpleTrigger(simple, model);
                    break;
                case TriggerType.Calendar:
                    var calTrigger = (ICalendarIntervalTrigger)trigger;
                    switch (calTrigger.MisfireInstruction)
                    {
                        case MisfireInstruction.CalendarIntervalTrigger.DoNothing:
                            model.MisfireAction = MisfireAction.DoNothing;
                            break;
                        case MisfireInstruction.CalendarIntervalTrigger.FireOnceNow:
                            model.MisfireAction = MisfireAction.FireOnceNow;
                            break;
                    }
                    model.TriggerInterval = calTrigger.RepeatInterval;
                    model.TriggerIntervalUnit = calTrigger.RepeatIntervalUnit.ToBlazingQuartzIntervalUnit();
                    model.InTimeZone = calTrigger.TimeZone;
                    break;
            }

            return model;
        }

        public async Task<bool> ContainsTriggerKey(string triggerName, string triggerGroup)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var trigger = await scheduler.GetTrigger(new TriggerKey(triggerName, triggerGroup));

            if (trigger == null)
                return false;

            return true;
        }

        public async Task<bool> ContainsJobKey(string jobName, string jobGroup)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jd = await scheduler.GetJobDetail(new JobKey(jobName, jobGroup));

            if (jd == null)
                return false;

            return true;
        }


        public async Task<IReadOnlyCollection<string>> GetCalendarNames(CancellationToken cancelToken = default)
        {
            var scheduler = await _schedulerFactory.GetScheduler(cancelToken);

            return await scheduler.GetCalendarNames(cancelToken);
        }

        #region Private methods

        private TriggerDetailModel PopulateSimpleTrigger(ISimpleTrigger simple, TriggerDetailModel model)
        {
            switch (simple.MisfireInstruction)
            {
                case MisfireInstruction.SimpleTrigger.RescheduleNextWithExistingCount:
                    model.MisfireAction = MisfireAction.RescheduleNextWithExistingCount;
                    break;
                case MisfireInstruction.SimpleTrigger.RescheduleNextWithRemainingCount:
                    model.MisfireAction = MisfireAction.RescheduleNextWithRemainingCount;
                    break;
                case MisfireInstruction.SimpleTrigger.RescheduleNowWithExistingRepeatCount:
                    model.MisfireAction = MisfireAction.RescheduleNowWithExistingRepeatCount;
                    break;
                case MisfireInstruction.SimpleTrigger.RescheduleNowWithRemainingRepeatCount:
                    model.MisfireAction = MisfireAction.RescheduleNowWithRemainingRepeatCount;
                    break;
                case MisfireInstruction.SimpleTrigger.FireNow:
                    model.MisfireAction = MisfireAction.FireNow;
                    break;
            }
            if (simple.RepeatCount >= 0)
                model.RepeatCount = simple.RepeatCount;
            else
                model.RepeatForever = true;

            var total = simple.RepeatInterval.TotalHours;
            if (Math.Round(total) == total)
            {
                model.TriggerInterval = Convert.ToInt32(total);
                model.TriggerIntervalUnit = IntervalUnit.Hour;
            }
            else
            {
                total = simple.RepeatInterval.TotalMinutes;
                if (Math.Round(total) == total)
                {
                    model.TriggerInterval = Convert.ToInt32(total);
                    model.TriggerIntervalUnit = IntervalUnit.Minute;
                }
                else
                {
                    total = simple.RepeatInterval.TotalSeconds;
                    if (Math.Round(total) == total)
                    {
                        model.TriggerInterval = Convert.ToInt32(total);
                        model.TriggerIntervalUnit = IntervalUnit.Second;
                    }
                    //else
                    //{
                    //    total = simple.RepeatInterval.TotalMilliseconds;
                    //    if (Math.Round(total) == total)
                    //    {
                    //        model.TriggerInterval = Convert.ToInt32(total);
                    //        model.TriggerIntervalUnit = IntervalUnit.Millisecond;
                    //    }
                    //}
                }
            }

            return model;
        }

        #endregion Private methods
    }
}

