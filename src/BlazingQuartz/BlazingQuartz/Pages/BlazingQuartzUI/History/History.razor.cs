using System;
using BlazingQuartz.Components;
using BlazingQuartz.Core.Data;
using BlazingQuartz.Core.Data.Entities;
using BlazingQuartz.Core.Models;
using BlazingQuartz.Core.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BlazingQuartz.Pages.BlazingQuartzUI.History
{
    public partial class History : ComponentBase
    {
        [Inject] private IDialogService DialogSvc { get; set; } = null!;
        [Inject] IExecutionLogService LogSvc { get; set; } = null!;

        private PagedList<ExecutionLog>? pagedData;
        private MudTable<ExecutionLog> table = null!;

        private long _firstLogId;

        private int totalItems;
        private bool _openFilter;

        private ExecutionLogFilter _filter = new();
        private ExecutionLogFilter _origFilter = new();
        private LogType? _selectedLogType;
        private IEnumerable<string> _jobNames = Enumerable.Empty<string>();
        private IEnumerable<string> _jobGroups = Enumerable.Empty<string>();
        private IEnumerable<string> _triggerNames = Enumerable.Empty<string>();
        private IEnumerable<string> _triggerGroups = Enumerable.Empty<string>();

        private async Task<TableData<ExecutionLog>> LoadExecutionLogs(TableState state)
        {
            PageMetadata pageMeta;
            if (pagedData == null)
            {
                pageMeta = new(0, state.PageSize);
            }
            else
            {
                pageMeta = pagedData.PageMetadata! with { Page = state.Page, PageSize = state.PageSize };
            }

            pagedData = await LogSvc.GetExecutionLogs(_filter,
                pageMeta, _firstLogId);

            if (pageMeta.Page == 0)
            {
                _firstLogId = pagedData.FirstOrDefault()?.LogId ?? 0;
            }

            ArgumentNullException.ThrowIfNull(pagedData.PageMetadata);

            totalItems = pagedData.PageMetadata.TotalCount;

            return new TableData<ExecutionLog>() {TotalItems = totalItems, Items = pagedData};
        }

        private void OnSearch(string? text)
        {
            _filter.MessageContains = text;
            RefreshLogs();
        }

        private void RefreshLogs()
        {
            pagedData = null;
            _firstLogId = 0;
            table.ReloadServerData();
        }

        private (string, Color, string) GetLogIconAndColor(ExecutionLog log)
        {
            if (log.IsException ?? false || (log.IsSuccess.HasValue && !log.IsSuccess.Value))
                return (Icons.Filled.Error, Color.Error, "Error");

            switch (log.LogType)
            {
                case Core.Data.LogType.ScheduleJob:
                    if (log.IsVetoed ?? false)
                        return (Icons.Filled.HighlightOff, Color.Warning, "Vetoed");

                    if (log.IsSuccess is null)
                    {
                        // still running
                        return (Icons.Filled.IncompleteCircle, Color.Secondary, "Executing");
                    }
                    else
                        return (Icons.Filled.Check, Color.Info, "Success");
                case Core.Data.LogType.Trigger:
                    return (Icons.Filled.Alarm, Color.Tertiary, "Trigger");
                default:
                    return (Icons.Outlined.Info, Color.Tertiary, "System Info");
            }
        }

        private void OnMoreDetails(ExecutionLog log, string title)
        {
            var options = new DialogOptions
            {
                CloseOnEscapeKey = true,
                FullWidth = true,
                MaxWidth = MaxWidth.Medium
            };

            var parameters = new DialogParameters
            {
                ["ExecutionLog"] = log
            };
            DialogSvc.Show<ExecutionDetailsDialog>(title, parameters, options);
        }

        #region Filters
        private async Task OnFilterClicked()
        {
            // backup original filter
            _origFilter = (ExecutionLogFilter)_filter.Clone();

            if (!_jobNames.Any())
            {
                // load filter
                await ReloadFilters();
            }

            _openFilter = true;
        }

        private void OnSaveFilter()
        {
            _openFilter = false;
        }

        private void OnClearFilter()
        {
            _filter = new();
            RefreshLogs();
            _openFilter = false;
        }

        private void OnCancelFilter()
        {
            _filter = _origFilter;
            RefreshLogs();
            _openFilter = false;
        }

        private async Task ReloadFilters()
        {
            _jobNames = await LogSvc.GetJobNames();
            _jobGroups = await LogSvc.GetJobGroups();
            _triggerNames = await LogSvc.GetTriggerNames();
            _triggerGroups = await LogSvc.GetTriggerGroups();
        }

        private void OnFilterJobGroupChanged(string? value)
        {
            _filter.JobGroup = value;
            RefreshLogs();
        }

        private void OnFilterJobNameChanged(string? value)
        {
            _filter.JobName = value;
            RefreshLogs();
        }

        private void OnFilterTriggerGroupChanged(string? value)
        {
            _filter.TriggerGroup = value;
            RefreshLogs();
        }

        private void OnFilterTriggerNameChanged(string? value)
        {
            _filter.TriggerName = value;
            RefreshLogs();
        }

        private void OnSelectedLogTypesChanged(LogType? logTypes)
        {
            _selectedLogType = logTypes;
            if (logTypes == null)
                _filter.LogTypes = null;
            else
                _filter.LogTypes = new HashSet<LogType> { logTypes.Value };

            RefreshLogs();
        }

        private void OnErrorOnlyChanged(bool errorOnly)
        {
            _filter.ErrorOnly = errorOnly;
            RefreshLogs();
        }

        private void OnIncludeSystemJobsChanged(bool flag)
        {
            _filter.IncludeSystemJobs = flag;
            RefreshLogs();
        }
        #endregion Filters
    }
}

