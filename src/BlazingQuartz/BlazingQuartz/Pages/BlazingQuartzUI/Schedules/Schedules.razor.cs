﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using BlazingQuartz.Components;
using BlazingQuartz.Core;
using BlazingQuartz.Core.Data;
using BlazingQuartz.Core.Events;
using BlazingQuartz.Core.Models;
using BlazingQuartz.Core.Services;
using BlazingQuartz.Jobs.Abstractions;
using BlazingQuartz.Extensions;
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
        [Inject] private IExecutionLogService ExecutionLogSvc { get; set; } = null!;
        [Inject] private IDialogService DialogSvc { get; set; } = null!;
        [Inject] private ILogger<Schedules> _logger { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;

        private ObservableCollection<ScheduleModel> ScheduledJobs { get; set; } = new();
        private string? SearchJobKeyword;
        private MudDataGrid<ScheduleModel>? _scheduleDataGrid;

        private bool _openFilter;

        private ScheduleJobFilter _filter = new();
        private ScheduleJobFilter _origFilter = new();

        internal bool IsEditActionDisabled(ScheduleModel model) => (model.JobStatus == JobStatus.NoSchedule ||
            model.JobStatus == JobStatus.Error ||
            model.JobGroup == Constants.SYSTEM_GROUP);

        internal bool IsRunActionDisabled(ScheduleModel model) => (model.JobStatus == JobStatus.NoSchedule ||
                                            model.JobStatus == JobStatus.NoTrigger);

        internal bool IsPauseActionDisabled(ScheduleModel model) => (model.JobStatus == JobStatus.NoSchedule ||
                                            model.JobStatus == JobStatus.Error ||
                                            model.JobStatus == JobStatus.NoTrigger);

        internal bool IsAddTriggerActionDisabled(ScheduleModel model) => model.JobStatus == JobStatus.NoSchedule ||
            model.JobStatus == JobStatus.Error ||
            model.JobGroup == Constants.SYSTEM_GROUP;

        internal bool IsCopyActionDisabled(ScheduleModel model) => (model.JobStatus == JobStatus.NoSchedule ||
            model.JobStatus == JobStatus.Error ||
            model.JobGroup == Constants.SYSTEM_GROUP);

        internal bool IsHistoryActionDisabled(ScheduleModel model) => model.JobStatus == JobStatus.NoSchedule;

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
                    if (model.JobName == null || model.JobStatus == JobStatus.Error)
                    {
                        // Just remove if no way to get job details
                        // if status is error, means get job details will throw exception
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
                    var isSuccess = e.JobExecutionContext.GetIsSuccess();
                    if (e.JobException != null)
                        model.ExceptionMessage = e.JobException.Message;
                    else if (isSuccess.HasValue && !isSuccess.Value)
                        model.ExceptionMessage = e.JobExecutionContext.GetReturnCodeAndResult();

                    StateHasChanged();
                }
            });
        }

        private async void SchedulerListenerSvc_OnJobScheduled(object? sender, EventArgs<ITrigger> e)
        {
            if (!_filter.IncludeSystemJobs && (e.Args.JobKey.Group == Constants.SYSTEM_GROUP || 
                e.Args.Key.Group == Constants.SYSTEM_GROUP))
            {
                // system job is not visible, skip this event
                return;
            }

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
                j.JobStatus != JobStatus.NoSchedule &&
                j.JobStatus != JobStatus.NoTrigger);
        }

        private IEnumerable<ScheduleModel> FindScheduleModel(JobKey jobKey, TriggerKey? triggerKey)
        {
            return ScheduledJobs.Where(j => j.Equals(jobKey, triggerKey)
                && ((j.JobStatus != JobStatus.NoSchedule && j.JobStatus != JobStatus.NoTrigger)
                    || (j.JobStatus == JobStatus.Error && j.TriggerName != null))
                );
        }

        async Task RefreshJobs()
        {
            ScheduledJobs.Clear();

            var jobs = SchedulerSvc.GetAllJobsAsync(_filter);
            await foreach(var job in jobs)
            {
                ScheduledJobs.Add(job);
            }
            if (ScheduledJobs.Any())
                _scheduleDataGrid?.ExpandAllGroups();

            await UpdateScheduleModelsLastExecution();
        }

        private async Task UpdateScheduleModelsLastExecution()
        {
            var latestResult = new PageMetadata(0, 1);
            var scheduleJobType = new HashSet<LogType> { LogType.ScheduleJob };

            foreach (var schModel in ScheduledJobs)
            {
                if (string.IsNullOrEmpty(schModel.JobName))
                    continue;

                var latestLogList = await ExecutionLogSvc.GetLatestExecutionLog(schModel.JobName, schModel.JobGroup,
                    schModel.TriggerName, schModel.TriggerGroup, latestResult,
                    logTypes: scheduleJobType);

                if (latestLogList != null && latestLogList.Any())
                {
                    var latestLog = latestLogList.First();
                    if (!schModel.PreviousTriggerTime.HasValue)
                    {
                        schModel.PreviousTriggerTime = latestLog.FireTimeUtc;
                    }
                    if (latestLog.IsSuccess.HasValue && !latestLog.IsSuccess.Value)
                    {
                        schModel.ExceptionMessage = latestLog.GetShortResultMessage();
                    }
                    else if (latestLog.IsException ?? false)
                    {
                        schModel.ExceptionMessage = latestLog.GetShortExceptionMessage();
                    }
                }   
            }
        }

        private Func<ScheduleModel, int, string> _scheduleRowStyleFunc => (model, i) =>
        {
            if (model.JobStatus == JobStatus.NoSchedule ||
                model.JobStatus == JobStatus.Error)
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
            
            try
            {
                await SchedulerSvc.CreateSchedule(jobDetail, triggerDetail);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Failed to create new schedule. {ex.Message}", Severity.Error);
                _logger.LogError(ex, "Failed to create new schedule.");
                // TODO show schedule dialog again?
            }
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
                {
                    origTriggerKey = new Key(currentTriggerModel.Name, currentTriggerModel.Group);

                    ResetStartEndDateTimeIfEarlier(ref currentTriggerModel);
                }
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
            try
            {
                await SchedulerSvc.UpdateSchedule(origJobKey, origTriggerKey,
                    jobDetail, triggerDetail);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Failed to update schedule. {ex.Message}", Severity.Error);
                _logger.LogError(ex, "Failed to update schedule.");
                // TODO display the dialog again?
            }
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
                else
                {
                    Snackbar.Add("Deleted schedule", Severity.Info);
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
                    ResetStartEndDateTimeIfEarlier(ref currentTriggerModel);
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

        private void OnJobHistory(ScheduleModel model)
        {
            if (model.JobName == null)
            {
                // not possible?
                return;
            }
            var options = new DialogOptions
            {
                CloseOnEscapeKey = true,
                FullWidth = true,
                MaxWidth = MaxWidth.Medium
            };

            var parameters = new DialogParameters
            {
                ["JobKey"] = new Key(model.JobName, model.JobGroup),
                ["TriggerKey"] = model.TriggerName != null ?
                    new Key(model.TriggerName, model.TriggerGroup ?? Constants.DEFAULT_GROUP) : null
            };
            var dlg = DialogSvc.Show<HistoryDialog>("Execution History", parameters, options);
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
            if (_scheduleDataGrid is null)
                return;

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
            var results = await Task.WhenAll(deleteTasks);

            if (results == null)
            {
                await RefreshJobs();
                Snackbar.Add("Failed to delete schedules", Severity.Error);
            }
            else
            {
                var deletedCount = results.Where(t => t).Count();
                var notDeletedCount = results.Count() - deletedCount;
                Snackbar.Add($"Deleted {deletedCount} schedule(s)", Severity.Info);

                if (notDeletedCount > 0)
                {
                    await RefreshJobs();
                    Snackbar.Add($"Failed to deleted {notDeletedCount} schedule(s)", Severity.Warning);
                }
            }
        }

        private void ResetStartEndDateTimeIfEarlier(ref TriggerDetailModel triggerModel)
        {
            var startDtime = triggerModel.StartDateTimeUtc;
            if (startDtime.HasValue && startDtime <= DateTimeOffset.UtcNow)
            {
                // clear start date if already past
                triggerModel.StartTimeSpan = null;
                triggerModel.StartDate = null;
                triggerModel.StartTimezone = TimeZoneInfo.Utc;
            }

            var endTime = triggerModel.EndDateTimeUtc;
            if (endTime.HasValue && endTime <= DateTimeOffset.UtcNow)
            {
                // clear end date if already past
                triggerModel.EndDate = null;
                triggerModel.EndTimeSpan = null;
            }
        }

        public void Dispose()
        {
            UnRegisterEventListeners();
        }

        #region Filter
        private void OnFilterClicked()
        {
            // backup original filter
            _origFilter = (ScheduleJobFilter)_filter.Clone();

            _openFilter = true;
        }

        private void OnSaveFilter()
        {
            _openFilter = false;
        }

        private async Task OnClearFilter()
        {
            _filter = new();
            await RefreshJobs();
            _openFilter = false;
        }

        private async Task OnCancelFilter()
        {
            _filter = _origFilter;
            await RefreshJobs();
            _openFilter = false;
        }

        private async Task OnIncludeSystemJobsChanged(bool value)
        {
            _filter.IncludeSystemJobs = value;
            await RefreshJobs();
        }
        #endregion Filter
    }
}

