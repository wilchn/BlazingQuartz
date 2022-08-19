using System;
using System.Globalization;
using System.Threading.Channels;
using BlazingQuartz.Core.Data;
using BlazingQuartz.Core.Data.Entities;
using BlazingQuartz.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BlazingQuartz.Core.History;

internal class SchedulerEventLoggingService : BackgroundService, ISchedulerEventLoggingService
{
    private const int MAX_BATCH_SIZE = 50;

    private readonly IServiceProvider _svcProvider;
    private readonly ISchedulerListenerService _schLisSvc;
    private readonly ILogger<SchedulerEventLoggingService> _logger;
    private readonly Channel<Func<IExecutionLogStore, CancellationToken, ValueTask>> _taskQueue;


    public SchedulerEventLoggingService(ILogger<SchedulerEventLoggingService> logger,
        IServiceProvider serviceProvider,
        ISchedulerListenerService listenerSvc)
    {
        _logger = logger;
        _svcProvider = serviceProvider;
        _schLisSvc = listenerSvc;
        _taskQueue = Channel.CreateUnbounded<Func<IExecutionLogStore, CancellationToken, ValueTask>>(
            new UnboundedChannelOptions
            {
                SingleReader = true
            });

        Init();
    }

    public override void Dispose()
    {
        _schLisSvc.OnJobToBeExecuted -= _schLisSvc_OnJobToBeExecuted;
        _schLisSvc.OnJobWasExecuted -= _schLisSvc_OnJobWasExecuted;
        _schLisSvc.OnJobExecutionVetoed -= _schLisSvc_OnJobExecutionVetoed;
        _schLisSvc.OnJobDeleted -= _schLisSvc_OnJobDeleted;
        _schLisSvc.OnJobAdded -= _schLisSvc_OnJobAdded;
        _schLisSvc.OnJobInterrupted -= _schLisSvc_OnJobInterrupted;
        _schLisSvc.OnSchedulerError -= _schLisSvc_OnSchedulerError;
        _schLisSvc.OnTriggerMisfired -= _schLisSvc_OnTriggerMisfired;
        _schLisSvc.OnTriggerPaused -= _schLisSvc_OnTriggerPaused;
        _schLisSvc.OnTriggerResumed -= _schLisSvc_OnTriggerResumed;
        _schLisSvc.OnTriggerFinalized -= _schLisSvc_OnTriggerFinalized;
        base.Dispose();
    }

    void Init()
    {
        _schLisSvc.OnJobToBeExecuted += _schLisSvc_OnJobToBeExecuted;
        _schLisSvc.OnJobWasExecuted += _schLisSvc_OnJobWasExecuted;
        _schLisSvc.OnJobExecutionVetoed += _schLisSvc_OnJobExecutionVetoed;
        _schLisSvc.OnJobDeleted += _schLisSvc_OnJobDeleted;
        _schLisSvc.OnJobAdded += _schLisSvc_OnJobAdded;
        _schLisSvc.OnJobInterrupted += _schLisSvc_OnJobInterrupted;
        _schLisSvc.OnSchedulerError += _schLisSvc_OnSchedulerError;
        _schLisSvc.OnTriggerMisfired += _schLisSvc_OnTriggerMisfired;
        _schLisSvc.OnTriggerPaused += _schLisSvc_OnTriggerPaused;
        _schLisSvc.OnTriggerResumed += _schLisSvc_OnTriggerResumed;
        _schLisSvc.OnTriggerFinalized += _schLisSvc_OnTriggerFinalized;
    }

    private void _schLisSvc_OnTriggerFinalized(object? sender, Events.EventArgs<ITrigger> e)
    {
        var jKey = e.Args.JobKey;
        var tKey = e.Args.Key;
        var log = new ExecutionLog
        {
            JobName = jKey.Name,
            JobGroup = jKey.Group,
            TriggerName = tKey.Name,
            TriggerGroup = tKey.Group,
            LogType = LogType.Trigger,
            Result = "Trigger ended"
        };
        QueueInsertTask(log);
    }

    private void _schLisSvc_OnTriggerResumed(object? sender, Events.EventArgs<TriggerKey> e)
    {
        var tKey = e.Args;
        var log = new ExecutionLog
        {
            TriggerName = tKey.Name,
            TriggerGroup = tKey.Group,
            LogType = LogType.Trigger,
            Result = "Trigger resumed"
        };
        QueueInsertTask(log);
    }

    private void _schLisSvc_OnTriggerPaused(object? sender, Events.EventArgs<TriggerKey> e)
    {
        var tKey = e.Args;
        var log = new ExecutionLog
        {
            TriggerName = tKey.Name,
            TriggerGroup = tKey.Group,
            LogType = LogType.Trigger,
            Result = "Trigger paused"
        };
        QueueInsertTask(log);
    }

    private void _schLisSvc_OnTriggerMisfired(object? sender, Events.EventArgs<ITrigger> e)
    {
        var jKey = e.Args.JobKey;
        var tKey = e.Args.Key;
        var log = new ExecutionLog
        {
            LogType = LogType.Trigger,
            JobName = jKey.Name,
            JobGroup = jKey.Group,
            TriggerName = tKey.Name,
            TriggerGroup = tKey.Group,
            Result = "Trigger misfired"
        };
        QueueInsertTask(log);
    }

    private void _schLisSvc_OnSchedulerError(object? sender, Events.SchedulerErrorEventArgs e)
    {
        var log = new ExecutionLog
        {
            LogType = LogType.System,
            IsException = true,
            ExceptionMessage = new ExceptionMessage
            {
                Message = e.ErrorMessage,
                StackTrace = e.Exception.StackTrace
            }
        };
        QueueInsertTask(log);
    }

    private void _schLisSvc_OnJobInterrupted(object? sender, Events.EventArgs<JobKey> e)
    {
        var jKey = e.Args;
        var log = new ExecutionLog
        {
            JobName = jKey.Name,
            JobGroup = jKey.Group,
            LogType = LogType.System,
            Result = "Job interrupted"
        };
        QueueInsertTask(log);
    }

    private void _schLisSvc_OnJobAdded(object? sender, Events.EventArgs<IJobDetail> e)
    {
        var jKey = e.Args.Key;
        var log = new ExecutionLog
        {
            JobName = jKey.Name,
            JobGroup = jKey.Group,
            LogType = LogType.System,
            Result = "Job added"
        };
        QueueInsertTask(log);
    }

    private void _schLisSvc_OnJobDeleted(object? sender, Events.EventArgs<JobKey> e)
    {
        JobKey jKey = e.Args;
        var log = new ExecutionLog
        {
            JobName = jKey.Name,
            JobGroup = jKey.Group,
            LogType = LogType.System,
            Result = "Job deleted"
        };
        QueueInsertTask(log);
    }

    private void _schLisSvc_OnJobExecutionVetoed(object? sender, Events.EventArgs<Quartz.IJobExecutionContext> e)
    {
        var log = CreateLogEntry(e.Args);
        log.IsVetoed = true;
        QueueUpdateTask(log);
    }

    private void _schLisSvc_OnJobWasExecuted(object? sender, Events.JobWasExecutedEventArgs e)
    {
        QueueUpdateTask(CreateLogEntry(e.JobExecutionContext, e.JobException));
    }

    private void _schLisSvc_OnJobToBeExecuted(object? sender, Events.EventArgs<Quartz.IJobExecutionContext> e)
    {
        QueueInsertTask(CreateLogEntry(e.Args));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var batch = await GetBatch(stoppingToken);

            _logger.LogInformation("Got a batch with {taskCount} task(s). Saving to data store.",
                batch.Count);

            try
            {
                using (IServiceScope scope = _svcProvider.CreateScope())
                {
                    var repo =
                        scope.ServiceProvider.GetRequiredService<IExecutionLogStore>();

                    foreach (var workItem in batch)
                    {
                        await workItem(repo, stoppingToken);
                    }

                    try
                    {
                        await repo.SaveChangesAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred while saving execution logs.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Prevent throwing if stoppingToken was signaled
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing task work item.");
            }
        }
    }



    private ExecutionLog CreateLogEntry(IJobExecutionContext context, JobExecutionException? jobException = null)
    {
        var log = new ExecutionLog
        {
            RunInstanceId = context.FireInstanceId,
            JobGroup = context.JobDetail.Key.Group,
            JobName = context.JobDetail.Key.Name,
            TriggerName = context.Trigger.Key.Name,
            TriggerGroup = context.Trigger.Key.Group,
            FireTimeUtc = context.FireTimeUtc,
            ScheduleFireTimeUtc = context.ScheduledFireTimeUtc,
            RetryCount = context.RefireCount,
            JobRunTime = context.JobRunTime,
            LogType = LogType.ScheduleJob
        };


        if (jobException != null)
        {
            log.ExceptionMessage = new ExceptionMessage
            {
                ErrorCode = jobException.HResult,
                Message = jobException.Message,
                StackTrace = jobException.StackTrace,
                HelpLink = jobException.HelpLink
            };
            log.IsException = true;
        }
        else
        {
            if (context.Result != null)
            {
                var result = Convert.ToString(context.Result, CultureInfo.InvariantCulture);
                log.Result = result;
            }
        }

        return log;
    }

    private async Task<List<Func<IExecutionLogStore, CancellationToken, ValueTask>>> GetBatch(CancellationToken cancellationToken)
    {
        await _taskQueue.Reader.WaitToReadAsync(cancellationToken);

        var batch = new List<Func<IExecutionLogStore, CancellationToken, ValueTask>>();

        while (batch.Count < MAX_BATCH_SIZE && _taskQueue.Reader.TryRead(out var dbTask))
        {
            batch.Add(dbTask);
        }

        return batch;
    }

    void QueueUpdateTask(ExecutionLog log)
    {
        QueueTask(async (IExecutionLogStore repo, CancellationToken cancelToken) =>
        {
            try
            {
                if (!repo.Exists(log))
                    await repo.SaveChangesAsync(cancelToken);

                await repo.UpdateExecutionLog(log);
            }
            catch (Exception ex)
            {
                if (log.LogType == LogType.ScheduleJob)
                {
                    _logger.LogError(ex,
                        "Error occurred while updating execution log with job key [{jobGroup}.{jobName}] " +
                        "run instance id [{runInstanceId}].", log.JobGroup, log.JobName, log.RunInstanceId);
                }
                else
                {
                    _logger.LogError(ex,
                        "Error occurred while updating {logType} execution log with " +
                        "run instance id [{runInstanceId}].", log.LogType,
                        log.RunInstanceId);
                }
            }

        });
    }

    void QueueInsertTask(ExecutionLog log)
    {
        QueueTask(async (IExecutionLogStore repo, CancellationToken cancelToken) =>
        {
            try
            {
                await repo.AddExecutionLog(log, cancelToken);
            }
            catch (Exception ex)
            {
                if (log.LogType == LogType.ScheduleJob)
                {
                    _logger.LogError(ex,
                        "Error occurred while adding execution log with job key [{jobGroup}.{jobName}] " +
                        "run instance id [{runInstanceId}].", log.JobGroup, log.JobName, log.RunInstanceId);
                }
                else
                {
                    _logger.LogError(ex,
                        "Error occurred while adding {logType} execution log with " +
                        "run instance id [{runInstanceId}].", log.LogType,
                        log.RunInstanceId);
                }
            }
            
        });
    }

    void QueueTask(Func<IExecutionLogStore, CancellationToken, ValueTask> task)
    {
        if (!_taskQueue.Writer.TryWrite(task))
        {
            // Should not happen since it's unbounded Channel. It 'should' only fail if we call writer.Complete()
            throw new InvalidOperationException("Failed to write the log message");
        }

    }
}

