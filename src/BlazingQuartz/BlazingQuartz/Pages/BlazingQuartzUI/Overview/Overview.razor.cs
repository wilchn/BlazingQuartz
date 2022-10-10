using System;
using BlazingQuartz.Components;
using BlazingQuartz.Core.Data;
using BlazingQuartz.Core.Data.Entities;
using BlazingQuartz.Core.Models;
using BlazingQuartz.Core.Services;
using BlazingQuartz.Extensions;
using Microsoft.AspNetCore.Components;
using System.Collections.Specialized;
using MudBlazor;

namespace BlazingQuartz.Pages.BlazingQuartzUI.Overview
{
    public partial class Overview : ComponentBase
    {
        const string UptimeKey = "Uptime";
        const string StatusKey = "Status";

        static double[] EmptyData = { 0 };
        static string[] EmptyLabel = { $"{JobExecutionStatus.Success} (0)" };

        [Inject] private IDialogService DialogSvc { get; set; } = null!;
        [Inject] IExecutionLogService LogSvc { get; set; } = null!;
        [Inject] ISchedulerService SchSvc { get; set; } = null!;

        private MudTable<ExecutionLog> table = null!;
        private ExecutionLogFilter _errorExecutionLogFilter = new ExecutionLogFilter 
        {
            ErrorOnly = true,
            LogTypes = new HashSet<LogType>{ LogType.ScheduleJob },
        };
        private PageMetadata errorLogPage = new PageMetadata(1, 10);
        private DateTimeOffset? _runningSince;

        private List<ExecutionLog> ErrorLogs = new();
        private OrderedDictionary SchedulerInfo = new();

        #region charts
        private int JobCount;
        private int TriggerCount;
        private int ExecutingCount;
        private int SysJobCount;
        private int SysTriggerCount;
        private int TotalLogDays;

        private static ChartOptions executionChartOptions = new ChartOptions
        {
            ChartPalette = new string[] { Colors.Green.Lighten1, Colors.Red.Lighten1, Colors.Blue.Lighten1, Colors.Grey.Lighten1 },
        };

        private static ChartOptions emptyExecutionChartOptions = new ChartOptions
        {
            ChartPalette = new string[] { Colors.Grey.Lighten1 },
            DisableLegend = true
        };

        private double[] TodaysLogData = EmptyData;
        private string[] TodaysLogLabel = EmptyLabel;
        private ChartOptions TodaysChartOptions = emptyExecutionChartOptions;
        private double[] YesterdaysLogData = EmptyData;
        private string[] YesterdaysLogLabel = EmptyLabel;
        private ChartOptions YesterdaysChartOptions = emptyExecutionChartOptions;
        private double[] AllTimeLogData = EmptyData;
        private string[] AllTimeLogLabel = EmptyLabel;
        private ChartOptions AllChartOptions = emptyExecutionChartOptions;
        #endregion charts

        protected override async Task OnInitializedAsync()
        {
            await Task.WhenAll(RefreshErrorLogs(),
                LoadInfo(),
                RefreshSchedulesCount(),
                RefreshLogSummary(),
                LoadYesterdaysLogSummary());
        }

        private async Task RefreshLogSummary()
        {
            var todayDateUtc = DateTime.Now.Date.ToUniversalTime();
            var yesterdayDateUtc = DateTime.Now.Date.AddDays(-1).ToUniversalTime();
            var today = await LogSvc.GetJobExecutionStatusSummary(
                todayDateUtc);
            var allTime = await LogSvc.GetJobExecutionStatusSummary(null);

            if (!today.Data.Any())
            {
                TodaysLogData = EmptyData;
                TodaysLogLabel = EmptyLabel;
                TodaysChartOptions = emptyExecutionChartOptions;
            }
            else
            {
                var chartData = ConvertToChartData(today.Data);
                TodaysLogData = chartData.Item1;
                TodaysLogLabel = chartData.Item2;
                TodaysChartOptions = executionChartOptions;
            }

            if (!allTime.Data.Any())
            {
                AllTimeLogData = EmptyData;
                AllTimeLogLabel = EmptyLabel;
                TotalLogDays = 0;
                AllChartOptions = emptyExecutionChartOptions;
            }
            else
            {
                var chartData = ConvertToChartData(allTime.Data);
                AllTimeLogData = chartData.Item1;
                AllTimeLogLabel = chartData.Item2;
                AllChartOptions = executionChartOptions;

                TotalLogDays = (int)Math.Round(DateTime.UtcNow.Subtract(allTime.StartDateTimeUtc).TotalDays);
            }
        }

        /// <summary>
        /// Yesterday's log summary. Pull this out from RefreshLogSummary() since this data won't change
        /// </summary>
        /// <returns></returns>
        private async Task LoadYesterdaysLogSummary()
        {
            var todayDateUtc = DateTime.Now.Date.ToUniversalTime();
            var yesterdayDateUtc = DateTime.Now.Date.AddDays(-1).ToUniversalTime();
            var yesterday = await LogSvc.GetJobExecutionStatusSummary(
                yesterdayDateUtc, todayDateUtc.AddMilliseconds(-1));

            if (!yesterday.Data.Any())
            {
                YesterdaysLogData = EmptyData;
                YesterdaysLogLabel = EmptyLabel;
                YesterdaysChartOptions = emptyExecutionChartOptions;
            }
            else
            {
                var chartData = ConvertToChartData(yesterday.Data);
                YesterdaysLogData = chartData.Item1;
                YesterdaysLogLabel = chartData.Item2;
                YesterdaysChartOptions = executionChartOptions;
            }
        }

        private (double[], string[]) ConvertToChartData(List<KeyValuePair<JobExecutionStatus, int>> data)
        {
            var values = new double[data.Count];
            var labels = new string[data.Count];

            for (int i = 0; i < data.Count; i++)
            {
                var entry = data[i];
                values[i] = entry.Value;
                labels[i] = $"{entry.Key} ({entry.Value})";
            }

            return (values, labels);
        }

        private async Task RefreshErrorLogs()
        {
            ErrorLogs = await LogSvc.GetExecutionLogs(_errorExecutionLogFilter,
                errorLogPage);
        }

        private async Task RefreshSchedulesCount()
        {
            var items = await SchSvc.GetScheduledJobSummary();
            foreach (var item in items)
            {
                switch (item.Key)
                {
                    case "Jobs":
                        JobCount = item.Value;
                        break;
                    case "Triggers":
                        TriggerCount = item.Value;
                        break;
                    case "Executing":
                        ExecutingCount = item.Value;
                        break;
                    case "System Jobs":
                        SysJobCount = item.Value;
                        break;
                    case "System Triggers":
                        SysTriggerCount = item.Value;
                        break;
                }
            }
            JobCount -= SysJobCount;
            TriggerCount -= SysTriggerCount;
        }

        private async Task LoadInfo()
        {
            var metadata = await SchSvc.GetMetadataAsync();
            if (metadata == null)
            {
                return;
            }

            SchedulerInfo.Clear();

            _runningSince = metadata.RunningSince;

            SchedulerInfo.Add(StatusKey, metadata.Started ? "Started" : 
                (metadata.InStandbyMode ? "Standby" : 
                    (metadata.Shutdown ? "Shutdown" : "Unknown")));
            SchedulerInfo.Add(UptimeKey, _runningSince.HasValue ? 
                DateTimeOffset.UtcNow.Subtract(_runningSince.Value).ToHumanTimeString() : "--");
            SchedulerInfo.Add("Scheduler Instance Id", metadata.SchedulerInstanceId);
            SchedulerInfo.Add("Scheduler Name", metadata.SchedulerName);
            SchedulerInfo.Add("Scheduler Remote", metadata.SchedulerRemote);
            SchedulerInfo.Add("Scheduler Type", metadata.SchedulerType);
            SchedulerInfo.Add("Thread Pool Size", metadata.ThreadPoolSize);
            SchedulerInfo.Add("Thread Pool Type", metadata.ThreadPoolType);
            SchedulerInfo.Add("Quartz Version", metadata.Version);
            SchedulerInfo.Add("BlazingQuartz Version", typeof(Overview).Assembly.GetName().Version);
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
    }
}

