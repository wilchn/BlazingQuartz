using System;
using System.Collections.ObjectModel;
using System.Linq;
using BlazingQuartz.Core;
using BlazingQuartz.Core.Events;
using BlazingQuartz.Core.Models;
using BlazingQuartz.Core.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Quartz;

namespace BlazingQuartz.Pages.BlazingQuartzUI.Schedules
{
    public partial class Schedules : ComponentBase, IDisposable
    {
        [Inject] private ISchedulerService SchedulerSvc { get; set; } = null!;
        [Inject] private ISchedulerListenerService SchedulerListenerSvc { get; set; } = null!;

        private ObservableCollection<ScheduleModel> ScheduledJobs { get; set; } = new();
        private string? SearchJobKeyword;

        // private TableGroupDefinition<ScheduleModel> _groupDefinition = new()
        // {
        //     GroupName = string.Empty,
        //     Indentation = false,
        //     Expandable = true,
        //     Selector = (e) => e.JobGroup
        // };

        Func<ScheduleModel, object> _groupDefinition = x => 
        {
            return x.JobGroup;
        };

        protected override async Task OnInitializedAsync()
        {
            RegisterEventListeners();
            await RefreshJobs();
        }

        private void RegisterEventListeners()
        {
            SchedulerListenerSvc.OnJobToBeExecuted += SchedulerListenerSvc_OnJobToBeExecuted;
            SchedulerListenerSvc.OnJobScheduled += SchedulerListenerSvc_OnJobScheduled;
            SchedulerListenerSvc.OnJobWasExecuted += SchedulerListenerSvc_OnJobWasExecuted;
        }

        private async void SchedulerListenerSvc_OnJobWasExecuted(object? sender, JobWasExecutedEventArgs e)
        {
            var jobKey = e.JobExecutionContext.JobDetail.Key;
            var triggerKey = e.JobExecutionContext.Trigger.Key;

            await InvokeAsync(() =>
            {
                var model = FindScheduleModel(jobKey, triggerKey);
                if (model is not null)
                {
                    model.PreviousTriggerTime = e.JobExecutionContext.FireTimeUtc;
                    model.NextTriggerTime = e.JobExecutionContext.NextFireTimeUtc;
                    model.JobStatus = JobStatus.Idle;

                    StateHasChanged();
                }
            });
        }

        private async void SchedulerListenerSvc_OnJobScheduled(object? sender, EventArgs<ITrigger> e)
        {
            await InvokeAsync(async () =>
            {
                var model = await SchedulerSvc.GetScheduleModelAsync(e.Args);
                ScheduledJobs.Add(model);
            });
        }

        private async void SchedulerListenerSvc_OnJobToBeExecuted(object? sender, EventArgs<IJobExecutionContext> e)
        {
            var jobKey = e.Args.JobDetail.Key;
            var triggerKey = e.Args.Trigger.Key;

            await InvokeAsync(() =>
            {
                var model = FindScheduleModel(jobKey, triggerKey);
                if (model is not null)
                {
                    model.JobStatus = JobStatus.Running;

                    StateHasChanged();
                }
            });
        }

        private void UnRegisterEventListeners()
        {
            SchedulerListenerSvc.OnJobToBeExecuted -= SchedulerListenerSvc_OnJobToBeExecuted;
            SchedulerListenerSvc.OnJobScheduled -= SchedulerListenerSvc_OnJobScheduled;
            SchedulerListenerSvc.OnJobWasExecuted -= SchedulerListenerSvc_OnJobWasExecuted;
        }

        private ScheduleModel? FindScheduleModel(JobKey jobKey, TriggerKey triggerKey)
        {
            return ScheduledJobs.Where(j => j.Equal(jobKey, triggerKey)).FirstOrDefault();
        }

        async Task RefreshJobs()
        {
            ScheduledJobs.Clear();

            var jobs = SchedulerSvc.GetAllJobsAsync();
            await foreach(var job in jobs)
            {
                ScheduledJobs.Add(job);
            }
        }

        private async Task OnNewSchedule()
        {
            await Task.CompletedTask;
        }

        public void Dispose()
        {
            UnRegisterEventListeners();
        }
    }
}

