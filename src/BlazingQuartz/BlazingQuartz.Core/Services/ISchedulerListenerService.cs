using System;
using Quartz;
using BlazingQuartz.Core.Events;

namespace BlazingQuartz.Core.Services
{
    public interface ISchedulerListenerService
    {
        event EventHandler<EventArgs<IJobDetail>>? OnJobAdded;
        event EventHandler<EventArgs<JobKey>>? OnJobDeleted;
        event EventHandler<EventArgs<IJobExecutionContext>>? OnJobExecutionVetoed;
        event EventHandler<EventArgs<JobKey>>? OnJobInterrupted;
        event EventHandler<EventArgs<JobKey>>? OnJobPaused;
        event EventHandler<EventArgs<JobKey>>? OnJobResumed;
        event EventHandler<EventArgs<ITrigger>>? OnJobScheduled;
        event EventHandler<EventArgs<string>>? OnJobsPaused;
        event EventHandler<EventArgs<string>>? OnJobsResumed;
        event EventHandler<EventArgs<IJobExecutionContext>>? OnJobToBeExecuted;
        event EventHandler<EventArgs<TriggerKey>>? OnJobUnscheduled;
        event EventHandler<JobWasExecutedEventArgs>? OnJobWasExecuted;
        event EventHandler<SchedulerErrorEventArgs>? OnSchedulerError;
        event EventHandler<CancellationToken>? OnSchedulerInStandbyMode;
        event EventHandler<CancellationToken>? OnSchedulerShutdown;
        event EventHandler<CancellationToken>? OnSchedulerShuttingdown;
        event EventHandler<CancellationToken>? OnSchedulerStarted;
        event EventHandler<CancellationToken>? OnSchedulerStarting;
        event EventHandler<CancellationToken>? OnSchedulingDataCleared;
        event EventHandler<EventArgs<ITrigger>>? OnTriggerFinalized;
        event EventHandler<EventArgs<ITrigger>>? OnTriggerMisfired;
        event EventHandler<EventArgs<TriggerKey>>? OnTriggerPaused;
        event EventHandler<EventArgs<TriggerKey>>? OnTriggerResumed;
        event EventHandler<EventArgs<string?>>? OnTriggerGroupPaused;
        event EventHandler<EventArgs<string?>>? OnTriggerGroupResumed;
        event EventHandler<TriggerEventArgs>? OnTriggerComplete;
        event EventHandler<TriggerEventArgs>? OnTriggerFired;
    }
}

