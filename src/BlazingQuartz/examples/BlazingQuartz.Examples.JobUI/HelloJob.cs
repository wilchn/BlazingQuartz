using System;
using BlazingQuartz.Jobs.Abstractions;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BlazingQuartz.Examples.JobUI;

public class HelloJob : IJob
{
    public const string PropertyMessage = "message";

    private readonly ILogger<HelloJob> _logger;
    private readonly IDataMapValueResolver _dmvResolver;

    public HelloJob(ILogger<HelloJob> logger,
        IDataMapValueResolver dmvResolver)
    {
        _logger = logger;
        // to support resolving dynamic variables
        _dmvResolver = dmvResolver;
    }

    public Task Execute(IJobExecutionContext context)
    {
        var rawMsg = context.GetDataMapValue(PropertyMessage);

        // resolve dynamic variable
        var msg = _dmvResolver.Resolve(rawMsg);

        _logger.LogInformation("Hello! {message}", msg);

        // Write the output to display in execution log
        context.Result = $"Hello! {msg}";
        context.JobDetail.JobDataMap[JobDataMapKeys.IsSuccess] = true;
        context.JobDetail.JobDataMap[JobDataMapKeys.ReturnCode] = 0;
        context.JobDetail.JobDataMap[JobDataMapKeys.ExecutionDetails] = "Executed successfully";

        return Task.CompletedTask;
    }
}

