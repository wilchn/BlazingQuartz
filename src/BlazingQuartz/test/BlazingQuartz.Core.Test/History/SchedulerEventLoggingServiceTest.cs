using System;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using BlazingQuartz.Core.Data.Entities;
using BlazingQuartz.Core.History;
using FluentAssertions;
using BlazingQuartz.Jobs.Abstractions;
using Moq;
using Quartz;

namespace BlazingQuartz.Core.Test.History;

public class SchedulerEventLoggingServiceTest
{
    [Theory, ServiceProviderAutoData]
    public async Task ProcessTaskAsync_JobExecutingToComplete_IsSuccessTrue(
        [Frozen] Mock<IExecutionLogStore> logStoreMock,
        [Frozen] Mock<IServiceProvider> serviceProviderMock,
        IJob job,
        Fixture fixture
        )
    {
        List<ExecutionLog> results = new();
        logStoreMock.Setup(ls => ls.AddExecutionLog(It.IsAny<ExecutionLog>(), It.IsAny<CancellationToken>()))
            .Callback((ExecutionLog log, CancellationToken token) => results.Add(log));
        logStoreMock.Setup(ls => ls.UpdateExecutionLog(It.IsAny<ExecutionLog>()))
            .Callback((ExecutionLog log) => results.Add(log));

        serviceProviderMock.Setup(s => s.GetService(typeof(IExecutionLogStore)))
            .Returns(logStoreMock.Object);

        var jobContext = TestUtils.NewJobExecutionContextFor(job);
        var sut = fixture.Create<SchedulerEventLoggingService>();

        // ACT
        sut._schLisSvc_OnJobToBeExecuted(this,
            new Events.EventArgs<IJobExecutionContext>(jobContext));
        sut._schLisSvc_OnJobWasExecuted(this,
            new Events.JobWasExecutedEventArgs(jobContext, null));
        await sut.ProcessTaskAsync();

        // ASSERT
        logStoreMock.Verify(repo => repo.AddExecutionLog(It.IsAny<ExecutionLog>(), It.IsAny<CancellationToken>()), Times.Once);
        logStoreMock.Verify(repo => repo.UpdateExecutionLog(It.IsAny<ExecutionLog>()), Times.Once);
        results.Count.Should().Be(2);

        results[0].IsSuccess.Should().BeNull();
        results[1].IsSuccess.Should().BeTrue();
    }

    [Theory, ServiceProviderAutoData]
    public async Task ProcessTaskAsync_JobExecutingToException_IsSuccessFalse(
        [Frozen] Mock<IExecutionLogStore> logStoreMock,
        [Frozen] Mock<IServiceProvider> serviceProviderMock,
        JobExecutionException exception,
        IJob job,
        Fixture fixture
        )
    {
        List<ExecutionLog> results = new();
        logStoreMock.Setup(ls => ls.AddExecutionLog(It.IsAny<ExecutionLog>(), It.IsAny<CancellationToken>()))
            .Callback((ExecutionLog log, CancellationToken token) => results.Add(log));
        logStoreMock.Setup(ls => ls.UpdateExecutionLog(It.IsAny<ExecutionLog>()))
            .Callback((ExecutionLog log) => results.Add(log));

        serviceProviderMock.Setup(s => s.GetService(typeof(IExecutionLogStore)))
            .Returns(logStoreMock.Object);

        var jobContext = TestUtils.NewJobExecutionContextFor(job);
        var sut = fixture.Create<SchedulerEventLoggingService>();

        // ACT
        sut._schLisSvc_OnJobToBeExecuted(this,
            new Events.EventArgs<IJobExecutionContext>(jobContext));
        sut._schLisSvc_OnJobWasExecuted(this,
            new Events.JobWasExecutedEventArgs(jobContext, exception));
        await sut.ProcessTaskAsync();

        // ASSERT
        logStoreMock.Verify(repo => repo.AddExecutionLog(It.IsAny<ExecutionLog>(), It.IsAny<CancellationToken>()), Times.Once);
        logStoreMock.Verify(repo => repo.UpdateExecutionLog(It.IsAny<ExecutionLog>()), Times.Once);
        results.Count.Should().Be(2);

        results[0].IsSuccess.Should().BeNull();
        results[1].IsSuccess.Should().BeFalse();
    }

    [Theory, ServiceProviderAutoData]
    public async Task ProcessTaskAsync_JobExecutingToCompleteButNotSuccess_IsSuccessFalse(
        [Frozen] Mock<IExecutionLogStore> logStoreMock,
        [Frozen] Mock<IServiceProvider> serviceProviderMock,
        IJob job,
        Fixture fixture
        )
    {
        List<ExecutionLog> results = new();
        logStoreMock.Setup(ls => ls.AddExecutionLog(It.IsAny<ExecutionLog>(), It.IsAny<CancellationToken>()))
            .Callback((ExecutionLog log, CancellationToken token) => results.Add(log));
        logStoreMock.Setup(ls => ls.UpdateExecutionLog(It.IsAny<ExecutionLog>()))
            .Callback((ExecutionLog log) => results.Add(log));

        serviceProviderMock.Setup(s => s.GetService(typeof(IExecutionLogStore)))
            .Returns(logStoreMock.Object);

        var jobContext = TestUtils.NewJobExecutionContextFor(job);
        var sut = fixture.Create<SchedulerEventLoggingService>();

        // ACT
        sut._schLisSvc_OnJobToBeExecuted(this,
            new Events.EventArgs<IJobExecutionContext>(jobContext));
        jobContext.SetIsSuccess(false);
        sut._schLisSvc_OnJobWasExecuted(this,
            new Events.JobWasExecutedEventArgs(jobContext, null));
        await sut.ProcessTaskAsync();

        // ASSERT
        logStoreMock.Verify(repo => repo.AddExecutionLog(It.IsAny<ExecutionLog>(), It.IsAny<CancellationToken>()), Times.Once);
        logStoreMock.Verify(repo => repo.UpdateExecutionLog(It.IsAny<ExecutionLog>()), Times.Once);
        results.Count.Should().Be(2);

        results[0].IsSuccess.Should().BeNull();
        results[1].IsSuccess.Should().BeFalse();
    }
}



