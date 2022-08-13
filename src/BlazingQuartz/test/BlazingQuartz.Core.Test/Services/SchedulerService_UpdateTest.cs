using System;
using System.Collections.Specialized;
using AutoFixture;
using BlazingQuartz.Core.Models;
using BlazingQuartz.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Quartz;
using Quartz.Impl;

namespace BlazingQuartz.Core.Test.Services
{
    public class SchedulerService_UpdateTest : IAsyncDisposable
    {
        ISchedulerFactory _factory;
        ISchedulerService _schedulerSvc;

        public SchedulerService_UpdateTest()
        {
            NameValueCollection properties = new NameValueCollection();
            properties["quartz.serializer.type"] = TestConstants.DefaultSerializerType;
            properties["quartz.scheduler.instanceName"] = "SchedulerService_UpdateTest";
            properties["quartz.scheduler.instanceId"] = "AUTO";
            _factory = new StdSchedulerFactory(properties);

            var loggerMock = new Mock<ILogger<SchedulerService>>();
            _schedulerSvc = new SchedulerService(loggerMock.Object, _factory);
        }

        public async ValueTask DisposeAsync()
        {
            var _scheduler = await _factory.GetScheduler();
            if (!_scheduler.IsShutdown)
                await _scheduler.Shutdown();
        }

        [Fact]
        public async Task UpdateSchedule_SimpleTriggerToCronTrigger_OneTrigger()
        {
            var fixture = new Fixture();
            var job = fixture.Build<JobDetailModel>()
                .With(j => j.JobClass, typeof(TestJob))
                .With(j => j.JobDataMap, new Dictionary<string, object> { { "JobOrig", "OrigValue" } })
                .Create();
            var trigger = new TriggerDetailModel
            {
                TriggerType = TriggerType.Simple,
                Name = fixture.Create<string>(),
                Group = fixture.Create<string>(),
                StartTimeSpan = new TimeSpan(5, 10, 15),
                StartDate = new DateTime(2022, 7, 1),
                EndTimeSpan = new TimeSpan(10, 5, 0),
                EndDate = new DateTime(2022, 7, 10),
                MisfireAction = MisfireAction.RescheduleNextWithExistingCount,
                Priority = 6,
                Description = fixture.Create<string>(),
                TriggerInterval = 2,
                TriggerIntervalUnit = IntervalUnit.Second,
                RepeatCount = 0,
                RepeatForever = true,
                TriggerDataMap = new Dictionary<string, object> { { "TriggerOrig", "OrigValue"} }
            };
            var scheduler = await _factory.GetScheduler();
            await scheduler.Start();
            await _schedulerSvc.CreateSchedule(job, trigger);
            var jobToUpdate = (await _schedulerSvc.GetJobDetail(job.Name, job.Group))!;
            var triggerToUpdate = (await _schedulerSvc.GetTriggerDetail(trigger.Name, trigger.Group))!;
            var schedListenerMock = new Mock<ISchedulerListener>();
            scheduler.ListenerManager.AddSchedulerListener(schedListenerMock.Object);

            jobToUpdate.Name = "New Job name";
            jobToUpdate.Group = "New Job group";
            jobToUpdate.JobDataMap = new Dictionary<string, object> { { "JobNew", "NewValue" } };

            triggerToUpdate.TriggerType = TriggerType.Cron;
            triggerToUpdate.CronExpression = "0 15 10 ? * *";
            await _schedulerSvc.UpdateSchedule(new Key(job.Name, job.Group), new Key(trigger.Name, trigger.Group),
                jobToUpdate, triggerToUpdate);

            var updatedJob = await _schedulerSvc.GetJobDetail(jobToUpdate.Name, jobToUpdate.Group);
            var updatedTrigger = await _schedulerSvc.GetTriggerDetail(triggerToUpdate.Name, triggerToUpdate.Group);
            var allJobs = await _schedulerSvc.GetAllJobsAsync().ToListAsync();

            await (await _factory.GetScheduler()).Shutdown();

            Assert.NotNull(updatedJob);
            Assert.NotNull(updatedTrigger);
            allJobs.Count().Should().Be(1);
            updatedJob.Name.Should().Be(jobToUpdate.Name);
            updatedJob.JobDataMap.ContainsKey("JobNew").Should().BeTrue();
            updatedTrigger.Name.Should().Be(triggerToUpdate.Name);
            updatedTrigger.TriggerType.Should().Be(TriggerType.Cron);
            schedListenerMock.Verify(l => l.JobUnscheduled(It.IsAny<TriggerKey>(), It.IsAny<CancellationToken>()),
                Times.Once, "Old trigger got unscheduled");
            schedListenerMock.Verify(l => l.JobScheduled(It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()),
                Times.Once, "Updated trigger and job got scheduled");
        }

        [Fact]
        public async Task UpdateSchedule_MultipleTriggers_SchedulerListenerSameTriggerCount()
        {
            var fixture = new Fixture();
            var job = fixture.Build<JobDetailModel>()
                .With(j => j.JobClass, typeof(TestJob))
                .With(j => j.JobDataMap, new Dictionary<string, object> { { "JobOrig", "OrigValue" } })
                .Create();
            var trigger = new TriggerDetailModel
            {
                TriggerType = TriggerType.Simple,
                Name = fixture.Create<string>(),
                Group = fixture.Create<string>(),
                TriggerInterval = 2,
                TriggerIntervalUnit = IntervalUnit.Second,
                RepeatForever = true,
                TriggerDataMap = new Dictionary<string, object> { { "TriggerOrig", "OrigValue" } }
            };
            var trigger2 = new TriggerDetailModel
            {
                TriggerType = TriggerType.Simple,
                Name = fixture.Create<string>(),
                Group = fixture.Create<string>(),
                TriggerInterval = 1,
                TriggerIntervalUnit = IntervalUnit.Second,
                RepeatForever = true,
            };
            var scheduler = await _factory.GetScheduler();
            await scheduler.Start();
            await _schedulerSvc.CreateSchedule(job, trigger);
            await _schedulerSvc.CreateSchedule(job, trigger2);
            var jobToUpdate = (await _schedulerSvc.GetJobDetail(job.Name, job.Group))!;
            var triggerToUpdate = (await _schedulerSvc.GetTriggerDetail(trigger.Name, trigger.Group))!;
            var schedListenerMock = new Mock<ISchedulerListener>();
            scheduler.ListenerManager.AddSchedulerListener(schedListenerMock.Object);

            jobToUpdate.Name = "New Job name";
            jobToUpdate.Group = "New Job group";
            jobToUpdate.JobDataMap = new Dictionary<string, object> { { "JobNew", "NewValue" } };

            triggerToUpdate.TriggerType = TriggerType.Cron;
            triggerToUpdate.CronExpression = "0 15 10 ? * *";
            await _schedulerSvc.UpdateSchedule(new Key(job.Name, job.Group), new Key(trigger.Name, trigger.Group),
                jobToUpdate, triggerToUpdate);

            var updatedJob = await _schedulerSvc.GetJobDetail(jobToUpdate.Name, jobToUpdate.Group);
            var updatedTrigger = await _schedulerSvc.GetTriggerDetail(triggerToUpdate.Name, triggerToUpdate.Group);
            var allJobs = await _schedulerSvc.GetAllJobsAsync().ToListAsync();

            await (await _factory.GetScheduler()).Shutdown();

            Assert.NotNull(updatedJob);
            Assert.NotNull(updatedTrigger);
            allJobs.Count().Should().Be(2, "After update schedule trigger count should not change");
            updatedJob.Name.Should().Be(jobToUpdate.Name);
            updatedJob.JobDataMap.ContainsKey("JobNew").Should().BeTrue();
            updatedTrigger.Name.Should().Be(triggerToUpdate.Name);
            updatedTrigger.TriggerType.Should().Be(TriggerType.Cron);
            schedListenerMock.Verify(l => l.JobUnscheduled(It.IsAny<TriggerKey>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2), "All triggers will be unscheduled");
            schedListenerMock.Verify(l => l.JobScheduled(It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2), "All triggers will get updated");
        }

    }
}

