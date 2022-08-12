using System;
using System.Collections.ObjectModel;
using System.Linq;
using BlazingQuartz.Components;
using BlazingQuartz.Core;
using BlazingQuartz.Core.Events;
using BlazingQuartz.Core.Models;
using BlazingQuartz.Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
using Quartz;

namespace BlazingQuartz.Pages.BlazingQuartzUI.Schedules
{
    public partial class Schedules : ComponentBase, IDisposable
    {
        [Inject] private ISchedulerService SchedulerSvc { get; set; } = null!;
        [Inject] private ISchedulerListenerService SchedulerListenerSvc { get; set; } = null!;
        [Inject] private IDialogService DialogSvc { get; set; } = null!;
        [Inject] private ILogger<Schedules> _logger { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;

        private ObservableCollection<ScheduleModel> ScheduledJobs { get; set; } = new();
        private string? SearchJobKeyword;
        private MudDataGrid<ScheduleModel> _scheduleDataGrid = null!;

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

        private void UnRegisterEventListeners()
        {
            SchedulerListenerSvc.OnJobToBeExecuted -= SchedulerListenerSvc_OnJobToBeExecuted;
            SchedulerListenerSvc.OnJobScheduled -= SchedulerListenerSvc_OnJobScheduled;
            SchedulerListenerSvc.OnJobWasExecuted -= SchedulerListenerSvc_OnJobWasExecuted;
            SchedulerListenerSvc.OnTriggerFinalized -= SchedulerListenerSvc_OnTriggerFinalized;
            SchedulerListenerSvc.OnJobDeleted -= SchedulerListenerSvc_OnJobDeleted;
            SchedulerListenerSvc.OnJobUnscheduled -= SchedulerListenerSvc_OnJobUnscheduled;
            SchedulerListenerSvc.OnTriggerResumed -= SchedulerListenerSvc_OnTriggerResumed;
            SchedulerListenerSvc.OnTriggerPaused -= SchedulerListenerSvc_OnTriggerPaused;
        }

        private void RegisterEventListeners()
        {
            SchedulerListenerSvc.OnJobToBeExecuted += SchedulerListenerSvc_OnJobToBeExecuted;
            SchedulerListenerSvc.OnJobScheduled += SchedulerListenerSvc_OnJobScheduled;
            SchedulerListenerSvc.OnJobWasExecuted += SchedulerListenerSvc_OnJobWasExecuted;
            SchedulerListenerSvc.OnTriggerFinalized += SchedulerListenerSvc_OnTriggerFinalized;
            SchedulerListenerSvc.OnJobDeleted += SchedulerListenerSvc_OnJobDeleted;
            SchedulerListenerSvc.OnJobUnscheduled += SchedulerListenerSvc_OnJobUnscheduled;
            SchedulerListenerSvc.OnTriggerResumed += SchedulerListenerSvc_OnTriggerResumed;
            SchedulerListenerSvc.OnTriggerPaused += SchedulerListenerSvc_OnTriggerPaused;
        }

        private async void SchedulerListenerSvc_OnTriggerPaused(object? sender, EventArgs<TriggerKey> e)
        {
            var triggerKey = e.Args;

            await InvokeAsync(() =>
            {
                var model = FindScheduleModelByTrigger(triggerKey).SingleOrDefault();
                if (model != null)
                {
                    model.JobStatus = JobStatus.Paused;
                    StateHasChanged();
                }
            });
        }

        private async void SchedulerListenerSvc_OnTriggerResumed(object? sender, EventArgs<TriggerKey> e)
        {
            var triggerKey = e.Args;

            await InvokeAsync(() =>
            {
                var model = FindScheduleModelByTrigger(triggerKey).SingleOrDefault();
                if (model != null)
                {
                    model.JobStatus = JobStatus.Idle;
                    StateHasChanged();
                }
            });
        }

        private async void SchedulerListenerSvc_OnJobUnscheduled(object? sender, EventArgs<TriggerKey> e)
        {
            _logger.LogInformation("Job trigger {triggerKey} got unscheduled", e.Args);
            await OnTriggerRemoved(e.Args);
        }

        private async void SchedulerListenerSvc_OnJobDeleted(object? sender, EventArgs<JobKey> e)
        {
            var jobKey = e.Args;
            _logger.LogInformation("Delete all schedule job {jobKey}", jobKey);

            await InvokeAsync(() =>
            {
                var modelList = ScheduledJobs.Where(s => s.JobName == jobKey.Name &&
                        s.JobGroup == jobKey.Group).ToList();
                modelList.ForEach(s => ScheduledJobs.Remove(s));
            });
        }

        private async void SchedulerListenerSvc_OnTriggerFinalized(object? sender, EventArgs<ITrigger> e)
        {
            var triggerKey = e.Args.Key;
            _logger.LogInformation("Trigger {triggerKey} finalized", triggerKey);

            await OnTriggerRemoved(triggerKey);
        }

        private async Task OnTriggerRemoved(TriggerKey triggerKey)
        {
            await InvokeAsync(async () =>
            {
                ScheduleModel? model;
                try
                {
                    model = FindScheduleModelByTrigger(triggerKey).SingleOrDefault();
                }
                catch (Exception ex)
                {
                    Snackbar.Add($"Cannot update trigger status. Found more than one schedule with trigger {triggerKey}", Severity.Warning);
                    _logger.LogWarning(ex, "Cannot update trigger status. Found more than one schedule with trigger {triggerKey}", triggerKey);
                    return;
                }
                
                if (model is not null)
                {
                    if (model.JobName == null)
                    {
                        ScheduledJobs.Remove(model);
                    }
                    else
                    {
                        var jobDetail = await SchedulerSvc.GetJobDetail(model.JobName, model.JobGroup);

                        if (jobDetail != null && jobDetail.IsDurable)
                        {
                            // see if similar job name already exists
                            var similarJobNameExists = ScheduledJobs.Any(s => s != model &&
                                s.JobName == model.JobName &&
                                s.JobGroup == model.JobGroup);
                            if (similarJobNameExists)
                            {
                                // delete this duplicate no trigger job
                                ScheduledJobs.Remove(model);
                            }
                            else
                            {
                                model.JobStatus = JobStatus.NoTrigger;
                                model.ClearTrigger();
                            }
                        }
                        else
                        {
                            model.JobStatus = JobStatus.NoSchedule;
                        }
                    }

                    StateHasChanged();
                }
            });
        }

        private async void SchedulerListenerSvc_OnJobWasExecuted(object? sender, JobWasExecutedEventArgs e)
        {
            var jobKey = e.JobExecutionContext.JobDetail.Key;
            var triggerKey = e.JobExecutionContext.Trigger.Key;

            await InvokeAsync(() =>
            {
                var model = FindScheduleModel(jobKey, triggerKey).SingleOrDefault();
                if (model is not null)
                {
                    model.PreviousTriggerTime = e.JobExecutionContext.FireTimeUtc;
                    model.NextTriggerTime = e.JobExecutionContext.NextFireTimeUtc;
                    model.JobStatus = JobStatus.Idle;
                    if (e.JobException != null)
                        model.ExceptionMessage = e.JobException.Message;

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
                var model = FindScheduleModel(jobKey, triggerKey).SingleOrDefault();
                if (model is not null)
                {
                    model.JobStatus = JobStatus.Running;

                    StateHasChanged();
                }
            });
        }

        private IEnumerable<ScheduleModel> FindScheduleModelByTrigger(TriggerKey triggerKey)
        {
            return ScheduledJobs.Where(j => j.EqualsTriggerKey(triggerKey) &&
                j.JobStatus != JobStatus.NoSchedule && j.JobStatus != JobStatus.NoTrigger);
        }

        private IEnumerable<ScheduleModel> FindScheduleModel(JobKey jobKey, TriggerKey? triggerKey)
        {
            return ScheduledJobs.Where(j => j.Equals(jobKey, triggerKey) &&
                j.JobStatus != JobStatus.NoSchedule && j.JobStatus != JobStatus.NoTrigger);
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

        private Func<ScheduleModel, int, string> _scheduleRowStyleFunc => (model, i) =>
        {
            if (model.JobStatus == JobStatus.NoSchedule)
                return "background-color:var(--mud-palette-background-grey)";

            return "";
        };

        private async Task OnNewSchedule()
        {
            var options = new DialogOptions {
                CloseOnEscapeKey = true,
                FullWidth = true,
                MaxWidth = MaxWidth.Medium
            };
            var dlg = DialogSvc.Show<ScheduleDialog>("Create Schedule Job", options);
            var result = await dlg.Result;

            if (result == null || result.Cancelled)
                return;

            // create schedule
            (JobDetailModel jobDetail, TriggerDetailModel triggerDetail) = ((JobDetailModel, TriggerDetailModel))result.Data;
            await SchedulerSvc.CreateSchedule(jobDetail, triggerDetail);
        }

        private async Task OnEditScheduleJob(ScheduleModel model)
        {
            if (model.JobName == null)
            {
                Snackbar.Add("Cannot edit schedule. Check if job still exists.", Severity.Error);
                return;
            }
            var currentJobDetail = await SchedulerSvc.GetJobDetail(model.JobName, model.JobGroup);

            if (currentJobDetail == null)
            {
                Snackbar.Add("Cannot edit schedule. Check if job still exists.", Severity.Error);
                return;
            }
            var origJobKey = new Key(currentJobDetail.Name, currentJobDetail.Group);

            TriggerDetailModel? currentTriggerModel = null;
            Key? origTriggerKey = null;
            if (model.TriggerName != null)
            {
                currentTriggerModel = await SchedulerSvc.GetTriggerDetail(model.TriggerName,
                    model?.TriggerGroup ?? Constants.DEFAULT_GROUP);

                if (currentTriggerModel != null)
                    origTriggerKey = new Key(currentTriggerModel.Name, currentTriggerModel.Group);
            }

            var options = new DialogOptions
            {
                CloseOnEscapeKey = true,
                FullWidth = true,
                MaxWidth = MaxWidth.Medium
            };
            var parameters = new DialogParameters
            {
                ["JobDetail"] = currentJobDetail,
                ["TriggerDetail"] = currentTriggerModel ?? new()
            };
            var dlg = DialogSvc.Show<ScheduleDialog>("Edit Schedule Job", parameters, options);
            var result = await dlg.Result;

            if (result == null || result.Cancelled)
                return;

            // update schedule
            (JobDetailModel jobDetail, TriggerDetailModel triggerDetail) = ((JobDetailModel, TriggerDetailModel))result.Data;
            await SchedulerSvc.UpdateSchedule(origJobKey, origTriggerKey,
                jobDetail, triggerDetail);
        }

        private async Task OnResumeScheduleJob(ScheduleModel model)
        {
            if (model.TriggerName == null)
            {
                Snackbar.Add("Cannot resume schedule. Trigger name is null.", Severity.Error);
                return;
            }

            await SchedulerSvc.ResumeTrigger(model.TriggerName, model.TriggerGroup);
        }

        private async Task OnPauseScheduleJob(ScheduleModel model)
        {
            if (model.TriggerName == null)
            {
                Snackbar.Add("Cannot pause schedule. Trigger name is null.", Severity.Error);
                return;
            }

            await SchedulerSvc.PauseTrigger(model.TriggerName, model.TriggerGroup);
        }

        private async Task OnDeleteScheduleJob(ScheduleModel model)
        {
            if (model.JobStatus == JobStatus.NoSchedule)
            {
                ScheduledJobs.Remove(model);
            }
            else
            {
                // confirm delete
                bool? yes = await DialogSvc.ShowMessageBox(
                    "Confirm Delete",
                    $"Do you want to delete this schedule?",
                    yesText: "Yes", cancelText: "No");
                if (yes == null || !yes.Value)
                {
                    return;
                }

                var success = await SchedulerSvc.DeleteSchedule(model);

                if (!success)
                {
                    Snackbar.Add($"Failed to delete schedule '{model.JobName}'", Severity.Error);
                }
            }
        }
        private async Task OnDuplicateScheduleJob(ScheduleModel model)
        {
            if (model.JobName == null)
            {
                Snackbar.Add("Cannot clone schedule. Check if job still exists.", Severity.Error);
                return;
            }
            var currentJobDetail = await SchedulerSvc.GetJobDetail(model.JobName, model.JobGroup);

            if (currentJobDetail == null)
            {
                Snackbar.Add("Cannot clone schedule. Check if job still exists.", Severity.Error);
                return;
            }

            TriggerDetailModel? currentTriggerModel = null;
            if (model.TriggerName != null)
            {
                currentTriggerModel = await SchedulerSvc.GetTriggerDetail(model.TriggerName,
                    model?.TriggerGroup ?? Constants.DEFAULT_GROUP);
                if (currentTriggerModel != null)
                {
                    currentTriggerModel.Name = string.Empty;
                }
            }

            currentJobDetail.Name = string.Empty;

            var options = new DialogOptions
            {
                CloseOnEscapeKey = true,
                FullWidth = true,
                MaxWidth = MaxWidth.Medium
            };
            var parameters = new DialogParameters
            {
                ["JobDetail"] = currentJobDetail,
                ["TriggerDetail"] = currentTriggerModel ?? new()
            };
            var dlg = DialogSvc.Show<ScheduleDialog>("Create Schedule Job", parameters, options);
            var result = await dlg.Result;

            if (result == null || result.Cancelled)
                return;

            // create schedule
            (JobDetailModel jobDetail, TriggerDetailModel triggerDetail) = ((JobDetailModel, TriggerDetailModel))result.Data;
            await SchedulerSvc.CreateSchedule(jobDetail, triggerDetail);
        }

        private async Task OnAddTrigger(ScheduleModel model)
        {
            if (model.JobName == null)
            {
                Snackbar.Add("Cannot add trigger. Check if job still exists.", Severity.Error);
                return;
            }
            var currentJobDetail = await SchedulerSvc.GetJobDetail(model.JobName, model.JobGroup);

            var options = new DialogOptions
            {
                CloseOnEscapeKey = true,
                FullWidth = true,
                MaxWidth = MaxWidth.Medium
            };
            var parameters = new DialogParameters
            {
                ["JobDetail"] = currentJobDetail,
                ["IsReadOnlyJobDetail"] = true,
                ["SelectedTab"] = ScheduleDialogTab.Trigger
            };
            var dlg = DialogSvc.Show<ScheduleDialog>("Add New Trigger", parameters, options);
            var result = await dlg.Result;

            if (result == null || result.Cancelled)
                return;

            // create schedule
            (JobDetailModel jobDetail, TriggerDetailModel triggerDetail) = ((JobDetailModel, TriggerDetailModel))result.Data;
            await SchedulerSvc.CreateSchedule(jobDetail, triggerDetail);
        }

        private async Task OnDeleteSelectedScheduleJobs()
        {
            var selectedItems = _scheduleDataGrid.SelectedItems;

            if (selectedItems == null || selectedItems.Count == 0)
                return;

            // confirm delete
            bool? yes = await DialogSvc.ShowMessageBox(
                "Confirm Delete",
                $"Do you want to delete selected {selectedItems.Count} schedules?",
                yesText: "Yes", cancelText: "No");
            if (yes == null || !yes.Value)
            {
                return;
            }

            var deleteTasks = selectedItems.Select(model =>
            {
                ScheduledJobs.Remove(model);
                return SchedulerSvc.DeleteSchedule(model);
            });
            await Task.WhenAll(deleteTasks);

            Snackbar.Add($"Deleted {selectedItems.Count} schedule(s)", Severity.Info);
        }

        public void Dispose()
        {
            UnRegisterEventListeners();
        }
    }
}

