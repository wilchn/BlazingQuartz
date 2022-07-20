using Microsoft.Extensions.Logging;
using Quartz;

namespace BlazingQuartz.Jobs;
public class HelloJob : IJob
{
    private readonly ILogger<HelloJob> _logger;
    public HelloJob(ILogger<HelloJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Hello world!");
        await Task.Delay(1000);
        //return Task.CompletedTask;
    }
}

