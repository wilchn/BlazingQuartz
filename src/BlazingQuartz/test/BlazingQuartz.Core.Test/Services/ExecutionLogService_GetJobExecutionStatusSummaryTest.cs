using System;
using AutoFixture;
using BlazingQuartz.Core.Data;
using BlazingQuartz.Core.Data.Entities;
using BlazingQuartz.Core.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BlazingQuartz.Core.Test.Services
{
    public class ExecutionLogService_GetJobExecutionStatusSummaryTest
    {
        private ExecutionLogService sut;
        private BlazingQuartzDbContext context;

        public ExecutionLogService_GetJobExecutionStatusSummaryTest()
        {
            context = CoreTestUtils.GetInMemoryBlazingQuartzDbContext(Guid.NewGuid().ToString());
            var contextFactoryMock = new Mock<IDbContextFactory<BlazingQuartzDbContext>>();
            contextFactoryMock.Setup(f => f.CreateDbContext()).
                Returns(context);
            sut = new ExecutionLogService(contextFactoryMock.Object);
        }

        [Fact]
        public async Task GetJobExecutionStatusSummary_ErrorStatus()
        {
            var fixture = new Fixture();
            context.ExecutionLogs.AddRange(
                fixture.Build<ExecutionLog>()
                    .With(l => l.LogType, LogType.ScheduleJob)
                    .With(l => l.DateAddedUtc, DateTimeOffset.UtcNow.Date)
                    .With(l => l.IsSuccess, false)
                    .Without(l => l.IsException)
                    .Without(l => l.IsVetoed)
                 .CreateMany(2));
            context.ExecutionLogs.Add(
                fixture.Build<ExecutionLog>()
                    .With(l => l.LogType, LogType.ScheduleJob)
                    .With(l => l.DateAddedUtc, DateTimeOffset.UtcNow.Date)
                    .With(l => l.IsException, true)
                    .With(l => l.IsVetoed, true)
                    .Without(l => l.IsSuccess)
                 .Create());
            context.ExecutionLogs.Add(
                fixture.Build<ExecutionLog>()
                    .With(l => l.LogType, LogType.ScheduleJob)
                    .With(l => l.DateAddedUtc, DateTimeOffset.UtcNow.Date)
                    .With(l => l.IsException, true)
                    .With(l => l.IsSuccess, true)
                    .Without(l => l.IsVetoed)
                 .Create());
            context.SaveChanges();

            var summary = await sut.GetJobExecutionStatusSummary(null);

            summary.Data.Count.Should().Be(1);
            summary.Data[0].Value.Should().Be(4);
        }

        [Fact]
        public async Task GetJobExecutionStatusSummary_EachOneStatus()
        {
            var fixture = new Fixture();
            context.ExecutionLogs.Add(
                fixture.Build<ExecutionLog>()
                    .With(l => l.LogType, LogType.ScheduleJob)
                    .With(l => l.DateAddedUtc, DateTimeOffset.UtcNow.Date)
                    .With(l => l.IsException, true)
                    .With(l => l.IsVetoed, true)
                    .Without(l => l.IsSuccess)
                 .Create());
            context.ExecutionLogs.Add(
                fixture.Build<ExecutionLog>()
                    .With(l => l.LogType, LogType.ScheduleJob)
                    .With(l => l.DateAddedUtc, DateTimeOffset.UtcNow.Date)
                    .Without(l => l.IsException)
                    .With(l => l.IsSuccess, true)
                    .Without(l => l.IsVetoed)
                 .Create());
            context.ExecutionLogs.Add(
                fixture.Build<ExecutionLog>()
                    .With(l => l.LogType, LogType.ScheduleJob)
                    .With(l => l.DateAddedUtc, DateTimeOffset.UtcNow.Date)
                    .Without(l => l.IsException)
                    .With(l => l.IsVetoed, true)
                    .With(l => l.IsSuccess, true)
                 .Create());
            context.ExecutionLogs.Add(
                fixture.Build<ExecutionLog>()
                    .With(l => l.LogType, LogType.ScheduleJob)
                    .With(l => l.DateAddedUtc, DateTimeOffset.UtcNow.Date)
                    .Without(l => l.IsException)
                    .Without(l => l.IsVetoed)
                    .Without(l => l.IsSuccess)
                 .Create());
            context.SaveChanges();

            var summary = await sut.GetJobExecutionStatusSummary(null);

            summary.Data.Count.Should().Be(4);
            summary.Data.Where(d => d.Key == JobExecutionStatus.Success).Count().Should().Be(1);
            summary.Data.Where(d => d.Key == JobExecutionStatus.Failed).Count().Should().Be(1);
            summary.Data.Where(d => d.Key == JobExecutionStatus.Executing).Count().Should().Be(1);
            summary.Data.Where(d => d.Key == JobExecutionStatus.Vetoed).Count().Should().Be(1);
        }

        [Fact]
        public async Task GetJobExecutionStatusSummary_EarliestStartDate()
        {
            var fixture = new Fixture();
            context.ExecutionLogs.Add(
                fixture.Build<ExecutionLog>()
                    .With(l => l.LogType, LogType.ScheduleJob)
                    .With(l => l.DateAddedUtc, DateTimeOffset.UtcNow.Date)
                    .With(l => l.IsException, true)
                    .With(l => l.IsVetoed, true)
                    .Without(l => l.IsSuccess)
                 .Create());
            context.ExecutionLogs.Add(
                fixture.Build<ExecutionLog>()
                    .With(l => l.LogType, LogType.ScheduleJob)
                    .With(l => l.DateAddedUtc, DateTimeOffset.UtcNow.Date.AddDays(-2))
                    .Without(l => l.IsException)
                    .With(l => l.IsSuccess, true)
                    .Without(l => l.IsVetoed)
                 .Create());
            context.SaveChanges();

            var summary = await sut.GetJobExecutionStatusSummary(null);

            summary.Data.Count.Should().Be(2);
            summary.Data.Where(d => d.Key == JobExecutionStatus.Success).Count().Should().Be(1);
            summary.Data.Where(d => d.Key == JobExecutionStatus.Failed).Count().Should().Be(1);
            summary.StartDateTimeUtc.Should().Be(DateTime.UtcNow.Date.AddDays(-2));
        }

        [Fact]
        public async Task GetJobExecutionStatusSummary_DateRange()
        {
            var fixture = new Fixture();
            context.ExecutionLogs.Add(
                fixture.Build<ExecutionLog>()
                    .With(l => l.LogType, LogType.ScheduleJob)
                    .With(l => l.DateAddedUtc, DateTimeOffset.UtcNow.Date)
                    .With(l => l.IsException, true)
                    .With(l => l.IsVetoed, true)
                    .Without(l => l.IsSuccess)
                 .Create());
            context.ExecutionLogs.Add(
                fixture.Build<ExecutionLog>()
                    .With(l => l.LogType, LogType.ScheduleJob)
                    .With(l => l.DateAddedUtc, DateTimeOffset.UtcNow.Date.AddDays(-1))
                    .Without(l => l.IsException)
                    .With(l => l.IsSuccess, true)
                    .Without(l => l.IsVetoed)
                 .Create());
            context.SaveChanges();

            var summary = await sut.GetJobExecutionStatusSummary(DateTimeOffset.UtcNow.Date.AddDays(-1),
                DateTimeOffset.UtcNow.Date.AddMilliseconds(-1));

            summary.Data.Count.Should().Be(1);
            summary.Data.Where(d => d.Key == JobExecutionStatus.Success).Count().Should().Be(1);
            summary.StartDateTimeUtc.Should().Be(DateTime.UtcNow.Date.AddDays(-1));
        }
    }
}

