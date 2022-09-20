using System;
using BlazingQuartz.Jobs.Abstractions;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BlazingQuartz.Examples.JobUI;

public class HelloJob : IJob
{
    public const string PropertyMessage = "message";

    private readonly ILogger<HelloJob> _logger;

    public HelloJob(ILogger<HelloJob> logger)
    {
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        var msg = context.MergedJobDataMap.GetString(PropertyMessage);

        _logger.LogInformation("Hello! {message}", msg);

        context.Result = $"Hello! {msg}";
        context.JobDetail.JobDataMap[JobDataMapKeys.IsSuccess] = true;
        context.JobDetail.JobDataMap[JobDataMapKeys.ReturnCode] = 0;
        context.JobDetail.JobDataMap[JobDataMapKeys.ExecutionDetails] = "Executed successfully";

        return Task.CompletedTask;
    }
}

