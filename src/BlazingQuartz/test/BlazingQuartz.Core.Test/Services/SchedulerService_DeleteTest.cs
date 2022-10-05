using System;
using System.Linq;
using System.Collections.Specialized;
using AutoFixture;
using BlazingQuartz.Core.Models;
using BlazingQuartz.Core.Services;
using FluentAssertions;
using Quartz;
using Quartz.Impl;
using Microsoft.Extensions.Logging;
using Moq;

namespace BlazingQuartz.Core.Test.Services
{
    public class SchedulerService_DeleteTest
    {
        ISchedulerFactory _factory;
        ISchedulerService _schedulerSvc;

        public SchedulerService_DeleteTest()
        {
            NameValueCollection properties = new NameValueCollection();
            properties["quartz.serializer.type"] = TestConstants.DefaultSerializerType;
            properties["quartz.scheduler.instanceName"] = "SchedulerService_DeleteTest";
            properties["quartz.scheduler.instanceId"] = "AUTO";
            _factory = new StdSchedulerFactory(properties);

            var loggerMock = new Mock<ILogger<SchedulerService>>();
            _schedulerSvc = new SchedulerService(loggerMock.Object, _factory);
        }

        [Fact]
        public async Task DeleteSchedule_DurableJobMultiTriggers_OneJobLeftAfterTriggerEnds()
        {
            var schedListenerMock = new Mock<ISchedulerListener>();
            var scheduler = await _factory.GetScheduler();
            scheduler.ListenerManager.AddSchedulerListener(schedListenerMock.Object);

            var fixture = new Fixture();
            var job = fixture.Build<JobDetailModel>()
                .With(j => j.JobClass, typeof(TestJob))
                .With(j => j.IsDurable, true)
                .Create();
            var trigger1 = new TriggerDetailModel
            {
                TriggerType = TriggerType.Simple,
                Name = fixture.Create<string>(),
                Group = fixture.Create<string>(),
                Description = fixture.Create<string>(),
                TriggerInterval = 1,
                TriggerIntervalUnit = IntervalUnit.Second,
                RepeatForever = false
            };
            var trigger2 = new TriggerDetailModel
            {
                TriggerType = TriggerType.Simple,
                Name = fixture.Create<string>(),
                Group = fixture.Create<string>(),
                Description = fixture.Create<string>(),
                TriggerInterval = 1,
                TriggerIntervalUnit = IntervalUnit.Second,
                RepeatForever = false
            };

            await scheduler.Start();
            await _schedulerSvc.CreateSchedule(job, trigger1);
            job.IsDurable = false; // does not affect
            await _schedulerSvc.CreateSchedule(job, trigger2);
            await Task.Delay(500);

            var schedList = await _schedulerSvc.GetAllJobsAsync().ToListAsync();
            var deleteSuccess = await _schedulerSvc.DeleteSchedule(schedList.First());
            var schedListAfterDelete = await _schedulerSvc.GetAllJobsAsync().ToListAsync();

            await Task.Delay(250);
            await (await _factory.GetScheduler()).Shutdown();

            deleteSuccess.Should().BeTrue();
            schedList.Count.Should().Be(1);
            schedList.First().TriggerName.Should().BeNull();
            schedListAfterDelete.Count.Should().Be(0);
            schedListenerMock.Verify(l => l.JobUnscheduled(It.IsAny<TriggerKey>(), It.IsAny<CancellationToken>()),
                Times.Never, "Nothing to unschedule since all trigger ends already when call DeleteSchedule()");
        }

        [Fact]
        public async Task DeleteSchedule_NoMoreTrigger_FireJobDeleted()
        {
            var schedListenerMock = new Mock<ISchedulerListener>();
            var scheduler = await _factory.GetScheduler();
            scheduler.ListenerManager.AddSchedulerListener(schedListenerMock.Object);

            var fixture = new Fixture();
            var job = fixture.Build<JobDetailModel>()
                .With(j => j.JobClass, typeof(TestJob))
                .With(j => j.IsDurable, true)
                .Create();
            var trigger1 = new TriggerDetailModel
            {
                TriggerType = TriggerType.Simple,
                Name = fixture.Create<string>(),
                Group = fixture.Create<string>(),
                TriggerInterval = 1,
                TriggerIntervalUnit = IntervalUnit.Second
            };

            await scheduler.Start();
            await _schedulerSvc.CreateSchedule(job, trigger1);
            await Task.Delay(500);

            var schedListBeforeDelete = await _schedulerSvc.GetAllJobsAsync().ToListAsync();
            var deleteResult = await _schedulerSvc.DeleteSchedule(schedListBeforeDelete.Single());
            var schedListAfterDelete = await _schedulerSvc.GetAllJobsAsync().ToListAsync();

            await Task.Delay(250);
            await (await _factory.GetScheduler()).Shutdown();

            deleteResult.Should().Be(true);
            schedListBeforeDelete.Single().JobStatus.Should().Be(JobStatus.NoTrigger);
            schedListAfterDelete.Count.Should().Be(0);
            schedListenerMock.Verify(l => l.JobUnscheduled(It.IsAny<TriggerKey>(), It.IsAny<CancellationToken>()),
                Times.Never, "Job has no trigger to unschedule");
            schedListenerMock.Verify(l => l.JobDeleted(It.IsAny<JobKey>(), It.IsAny<CancellationToken>()),
                Times.Once, "JobDeleted will be fired when no trigger job got deleted");
        }

        [Fact]
        public async Task DeleteSchedule_StillHasOtherTriggerDeleteNoTrigger_ShouldNotDeleteOtherTriggers()
        {
            var schedListenerMock = new Mock<ISchedulerListener>();
            var scheduler = await _factory.GetScheduler();
            scheduler.ListenerManager.AddSchedulerListener(schedListenerMock.Object);

            var fixture = new Fixture();
            var job = fixture.Build<JobDetailModel>()
                .With(j => j.JobClass, typeof(TestJob))
                .With(j => j.IsDurable, true)
                .Create();
            var triggerForever = new TriggerDetailModel
            {
                TriggerType = TriggerType.Simple,
                Name = fixture.Create<string>(),
                Group = fixture.Create<string>(),
                TriggerInterval = 1,
                TriggerIntervalUnit = IntervalUnit.Second,
                RepeatForever = true
            };
            var trigger1 = new TriggerDetailModel
            {
                TriggerType = TriggerType.Simple,
                Name = fixture.Create<string>(),
                Group = fixture.Create<string>(),
                TriggerInterval = 1,
                TriggerIntervalUnit = IntervalUnit.Second
            };

            await _schedulerSvc.CreateSchedule(job, triggerForever);
            await _schedulerSvc.CreateSchedule(job, trigger1);
            var allSchedules = await _schedulerSvc.GetAllJobsAsync().ToListAsync();
            var triggerOnceModel = allSchedules.Where(m => m.TriggerName == trigger1.Name).First();
            triggerOnceModel.JobStatus = JobStatus.NoTrigger;
            await scheduler.Start();
            await Task.Delay(1000);

            var schedListBeforeDelete = await _schedulerSvc.GetAllJobsAsync().ToListAsync();
            await _schedulerSvc.DeleteSchedule(triggerOnceModel);
            var schedListAfterDelete = await _schedulerSvc.GetAllJobsAsync().ToListAsync();

            await (await _factory.GetScheduler()).Shutdown();

            allSchedules.Count.Should().Be(2);
            schedListBeforeDelete.Count.Should().Be(1);
            schedListAfterDelete.Count.Should().Be(1);
            schedListenerMock.Verify(l => l.TriggerFinalized(It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()),
                Times.Once, "1 trigger run once only");
            schedListenerMock.Verify(l => l.JobDeleted(It.IsAny<JobKey>(), It.IsAny<CancellationToken>()),
                Times.Never, "JobDeleted will not be fired since there are still triggers assigned to the job");
        }
    }
}

