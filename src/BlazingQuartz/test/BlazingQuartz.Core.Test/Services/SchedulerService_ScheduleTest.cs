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

namespace BlazingQuartz.Core.Test.Services;

public class SchedulerService_ScheduleTest
{
    ISchedulerFactory _factory;
    ISchedulerService _schedulerSvc;

    public SchedulerService_ScheduleTest()
    {
        NameValueCollection properties = new NameValueCollection();
        properties["quartz.serializer.type"] = TestConstants.DefaultSerializerType;
        properties["quartz.scheduler.instanceName"] = "SchedulerService_ScheduleTest";
        properties["quartz.scheduler.instanceId"] = "AUTO";
        _factory = new StdSchedulerFactory(properties);

        var loggerMock = new Mock<ILogger<SchedulerService>>();
        _schedulerSvc = new SchedulerService(loggerMock.Object, _factory);
    }

    [Fact]
    public async Task ScheduleJobAndGetDetail_CalendarTrigger()
    {
        var fixture = new Fixture();
        var job = fixture.Build<JobDetailModel>()
            .With(j => j.JobClass, typeof(TestJob))
            .Create();
        var trigger = new TriggerDetailModel
        {
            TriggerType = TriggerType.Calendar,
            Name = fixture.Create<string>(),
            Group = fixture.Create<string>(),
            StartTimeSpan = new TimeSpan(5, 10, 15),
            StartDate = new DateTime(2022, 7, 1),
            EndTimeSpan = new TimeSpan(10, 5, 0),
            EndDate = new DateTime(2022, 7, 10),
            MisfireAction = MisfireAction.FireOnceNow,
            Priority = 6,
            Description = fixture.Create<string>(),
            TriggerInterval = 2,
            TriggerIntervalUnit = IntervalUnit.Day,
            InTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time")
            //TODO calendar name
        };

        await _schedulerSvc.CreateSchedule(job, trigger);
        var jobResult = await _schedulerSvc.GetJobDetail(job.Name, job.Group);
        var triggerResult = await _schedulerSvc.GetTriggerDetail(trigger.Name, trigger.Group);

        await (await _factory.GetScheduler()).Shutdown();

        jobResult.Should().BeEquivalentTo(job);
        triggerResult.Should().BeEquivalentTo(trigger);
    }

    [Fact]
    public async Task ScheduleJobAndGetDetail_SimpleTrigger()
    {
        var fixture = new Fixture();
        var job = fixture.Build<JobDetailModel>()
            .With(j => j.JobClass, typeof(TestJob))
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
            RepeatForever = true
            //TODO calendar name
        };

        await _schedulerSvc.CreateSchedule(job, trigger);
        var jobResult = await _schedulerSvc.GetJobDetail(job.Name, job.Group);
        var triggerResult = await _schedulerSvc.GetTriggerDetail(trigger.Name, trigger.Group);

        await (await _factory.GetScheduler()).Shutdown();

        jobResult.Should().BeEquivalentTo(job);
        triggerResult.Should().BeEquivalentTo(trigger);
    }

    [Fact]
    public async Task ScheduleJobAndGetDetail_DailyTrigger()
    {
        var fixture = new Fixture();
        var job = fixture.Build<JobDetailModel>()
            .With(j => j.JobClass, typeof(TestJob))
            .Create();
        var trigger = new TriggerDetailModel
        {
            TriggerType = TriggerType.Daily,
            Name = fixture.Create<string>(),
            Group = fixture.Create<string>(),
            StartTimeSpan = new TimeSpan(5, 10, 15),
            StartDate = new DateTime(2022, 7, 1),
            EndTimeSpan = new TimeSpan(10, 5, 0),
            EndDate = new DateTime(2022, 7, 10),
            MisfireAction = MisfireAction.FireOnceNow,
            Priority = 6,
            Description = fixture.Create<string>(),
            TriggerInterval = 2,
            TriggerIntervalUnit = IntervalUnit.Second,
            StartDailyTime = new TimeSpan(14, 12, 10),
            EndDailyTime = new TimeSpan(16, 15, 20),
            InTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"),
            DailyDayOfWeek = new bool[7]
            {
                true, false, true, false, true, true, true
            }
            //TODO calendar name
        };

        await _schedulerSvc.CreateSchedule(job, trigger);
        var jobResult = await _schedulerSvc.GetJobDetail(job.Name, job.Group);
        var triggerResult = await _schedulerSvc.GetTriggerDetail(trigger.Name, trigger.Group);

        await (await _factory.GetScheduler()).Shutdown();

        jobResult.Should().BeEquivalentTo(job);
        triggerResult.Should().BeEquivalentTo(trigger);
    }

    [Fact]
    public async Task ScheduleJobAndGetDetail_CronTrigger()
    {
        var fixture = new Fixture();
        var job = fixture.Build<JobDetailModel>()
            .With(j => j.JobClass, typeof(TestJob))
            .Create();
        var trigger = new TriggerDetailModel
        {
            TriggerType = TriggerType.Cron,
            Name = fixture.Create<string>(),
            Group = fixture.Create<string>(),
            StartTimeSpan = new TimeSpan(5, 10, 15),
            StartDate = new DateTime(2022, 7, 1),
            EndTimeSpan = new TimeSpan(10, 5, 0),
            EndDate = new DateTime(2022, 7, 10),
            MisfireAction = MisfireAction.FireOnceNow,
            Priority = 6,
            Description = fixture.Create<string>(),
            InTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"),
            CronExpression = "0 0 03-07 ? * MON-FRI"
            //TODO calendar name
        };

        await _schedulerSvc.CreateSchedule(job, trigger);
        var jobResult = await _schedulerSvc.GetJobDetail(job.Name, job.Group);
        var triggerResult = await _schedulerSvc.GetTriggerDetail(trigger.Name, trigger.Group);

        await (await _factory.GetScheduler()).Shutdown();

        jobResult.Should().BeEquivalentTo(job);
        triggerResult.Should().BeEquivalentTo(trigger);
    }

    [Fact]
    public async Task ScheduleJobAndGetDetail_CronTriggerWithDataMap()
    {
        var fixture = new Fixture();
        var job = fixture.Build<JobDetailModel>()
            .With(j => j.JobClass, typeof(TestJob))
            .With(j => j.JobDataMap, new Dictionary<string, object>
            {
                { fixture.Create<string>(), fixture.Create<int>() },
                { fixture.Create<string>(), fixture.Create<string>() },
                { fixture.Create<string>(), fixture.Create<decimal>() },
                { fixture.Create<string>(), fixture.Create<bool>() },
            })
            .Create();
        var trigger = new TriggerDetailModel
        {
            TriggerType = TriggerType.Cron,
            Name = fixture.Create<string>(),
            Group = fixture.Create<string>(),
            StartTimeSpan = new TimeSpan(5, 10, 15),
            StartDate = new DateTime(2022, 7, 1),
            EndTimeSpan = new TimeSpan(10, 5, 0),
            EndDate = new DateTime(2022, 7, 10),
            MisfireAction = MisfireAction.FireOnceNow,
            Priority = 6,
            Description = fixture.Create<string>(),
            InTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"),
            CronExpression = "0 0 03-07 ? * MON-FRI",
            TriggerDataMap = new Dictionary<string, object>
            {
                { fixture.Create<string>(), fixture.Create<int>() },
                { fixture.Create<string>(), fixture.Create<string>() },
                { fixture.Create<string>(), fixture.Create<decimal>() },
                { fixture.Create<string>(), fixture.Create<bool>() },
            }
            //TODO calendar name
        };

        await _schedulerSvc.CreateSchedule(job, trigger);
        var jobResult = await _schedulerSvc.GetJobDetail(job.Name, job.Group);
        var triggerResult = await _schedulerSvc.GetTriggerDetail(trigger.Name, trigger.Group);

        await (await _factory.GetScheduler()).Shutdown();

        jobResult.Should().BeEquivalentTo(job);
        triggerResult.Should().BeEquivalentTo(trigger);
    }

}

