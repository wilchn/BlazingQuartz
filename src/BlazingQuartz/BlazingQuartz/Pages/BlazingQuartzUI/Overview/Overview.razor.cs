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
    public partial class Overview : ComponentBase, IDisposable
    {
        const string UptimeKey = "Uptime";
        const string StatusKey = "Status";
        const string STARTED = "Started";
        const string STARTING = "Starting";
        const string STANDBY = "Standby";
        const string SHUTDOWN = "Shutdown";

        static double[] EmptyData = { 0 };
        static string[] EmptyLabel = { $"{JobExecutionStatus.Success} (0)" };

        [Inject] private IDialogService DialogSvc { get; set; } = null!;
        [Inject] IExecutionLogService LogSvc { get; set; } = null!;
        [Inject] ISchedulerService SchSvc { get; set; } = null!;
        [Inject] ISchedulerListenerService SchLisSvc { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;

        private MudTable<ExecutionLog> table = null!;
        private ExecutionLogFilter _errorExecutionLogFilter = new ExecutionLogFilter 
        {
            ErrorOnly = true,
            LogTypes = new HashSet<LogType>{ LogType.ScheduleJob },
        };
        private PageMetadata errorLogPage = new PageMetadata(0, 10);
        private DateTimeOffset? RunningSince;

        private List<ExecutionLog> ErrorLogs = new();
        private OrderedDictionary SchedulerInfo = new();

        private bool IsPauseResumeDisabled = false;
        private bool IsStartStandbyDisabled = false;
        private bool IsStartButtonVisible = false;
        private bool IsShutdown = false;

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

        private DateTimeOffset lastCaptureDate = DateTimeOffset.Now.Date;
        #endregion charts

        #region Refresh timer
        private Timer? _refreshTimer;
        private const int REFRESH_IN_MS = 10000;
        private bool AutoRefresh = true;
        #endregion Refresh timer

        protected override async Task OnInitializedAsync()
        {
            await LoadInfo();
            RegisterEventListeners();

            await Task.WhenAll(RefreshErrorLogs(),
                RefreshSchedulesCount(),
                RefreshLogSummary(),
                LoadYesterdaysLogSummary());

            _refreshTimer = new Timer(async (_) =>
            {
                await InvokeAsync(async () =>
                {
                    //_logger.LogInformation("{time} Refresh trade summary", DateTime.Now);
                    RefreshUptime();
                    await Task.WhenAll(RefreshErrorLogs(),
                        RefreshSchedulesCount(),
                        RefreshLogSummary());

                    // Update the UI
                    StateHasChanged();
                });
            }, null,
            IsPauseResumeDisabled ? Timeout.Infinite : REFRESH_IN_MS,
            REFRESH_IN_MS);

        }

        private void UnRegisterEventListeners()
        {
            SchLisSvc.OnSchedulerInStandbyMode -= SchLisSvc_OnSchedulerInStandbyMode;
            SchLisSvc.OnSchedulerShutdown -= SchLisSvc_OnSchedulerShutdown;
            SchLisSvc.OnSchedulerStarted -= SchLisSvc_OnSchedulerStarted;
            SchLisSvc.OnSchedulerStarting -= SchLisSvc_OnSchedulerStarting;
        }

        private void RegisterEventListeners()
        {
            SchLisSvc.OnSchedulerInStandbyMode += SchLisSvc_OnSchedulerInStandbyMode;
            SchLisSvc.OnSchedulerShutdown += SchLisSvc_OnSchedulerShutdown;
            SchLisSvc.OnSchedulerStarted += SchLisSvc_OnSchedulerStarted;
            SchLisSvc.OnSchedulerStarting += SchLisSvc_OnSchedulerStarting;
        }

        private async void SchLisSvc_OnSchedulerStarting(object? sender, CancellationToken e)
        {
            await InvokeAsync(() =>
            {
                SchedulerInfo[StatusKey] = STARTING;
                IsStartStandbyDisabled = true;

                StateHasChanged();
            });
        }

        private async void SchLisSvc_OnSchedulerStarted(object? sender, CancellationToken e)
        {
            await InvokeAsync(async () =>
            {
                Snackbar.Add("Scheduler started", Severity.Info);
                await LoadInfo();
                IsStartStandbyDisabled = false;
                StartAutoRefresh();

                StateHasChanged();
            });
        }

        private async void SchLisSvc_OnSchedulerShutdown(object? sender, CancellationToken e)
        {
            await InvokeAsync(async () =>
            {
                Snackbar.Add("Scheduler was shutdown", Severity.Info);
                await LoadInfo();
                StopAutoRefresh();

                StateHasChanged();
            });
        }

        private async void SchLisSvc_OnSchedulerInStandbyMode(object? sender, CancellationToken e)
        {
            await InvokeAsync(async () =>
            {
                Snackbar.Add("Scheduler in standby mode", Severity.Info);
                await LoadInfo();
                StopAutoRefresh();

                StateHasChanged();
            });

        }

        private async Task RefreshLogSummary()
        {
            var todayDateUtc = DateTime.Now.Date.ToUniversalTime();
            var today = await LogSvc.GetJobExecutionStatusSummary(
                todayDateUtc);
            var allTime = await LogSvc.GetJobExecutionStatusSummary(null);
            var nowDate = DateTimeOffset.Now.Date;

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

            if (nowDate != lastCaptureDate)
            {
                await LoadYesterdaysLogSummary();
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

                TotalLogDays = (int)Math.Round(DateTime.Now.Subtract(
                    DateTime.SpecifyKind(allTime.StartDateTimeUtc, DateTimeKind.Utc).ToLocalTime()).TotalDays);
            }

            lastCaptureDate = nowDate;
        }

        /// <summary>
        /// Yesterday's log summary. Separated from <see cref="RefreshLogSummary"/> since only need to
        /// call this when <see cref="lastCaptureDate"/> is different
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

        private void RefreshUptime()
        {
            SchedulerInfo[UptimeKey] = RunningSince.HasValue ?
                DateTimeOffset.UtcNow.Subtract(RunningSince.Value).ToHumanTimeString() : "--";
        }

        private async Task LoadInfo()
        {
            var metadata = await SchSvc.GetMetadataAsync();
            if (metadata == null)
            {
                return;
            }

            SchedulerInfo.Clear();

            RunningSince = metadata.RunningSince;

            IsStartButtonVisible = metadata.InStandbyMode || metadata.Shutdown;
            IsShutdown = metadata.Shutdown;
            IsPauseResumeDisabled = IsStartButtonVisible || IsShutdown;

            SchedulerInfo.Add("Quartz Version", metadata.Version);
            SchedulerInfo.Add("BlazingQuartz Version", typeof(Overview).Assembly.GetName().Version);
            SchedulerInfo.Add(StatusKey, metadata.Shutdown ? SHUTDOWN : 
                (metadata.InStandbyMode ? STANDBY : 
                    (metadata.Started ? STARTED : "Unknown")));
            SchedulerInfo.Add(UptimeKey, RunningSince.HasValue ? 
                DateTimeOffset.UtcNow.Subtract(RunningSince.Value).ToHumanTimeString() : "--");
            SchedulerInfo.Add("Scheduler Instance Id", metadata.SchedulerInstanceId);
            SchedulerInfo.Add("Scheduler Name", metadata.SchedulerName);
            SchedulerInfo.Add("Scheduler Remote", metadata.SchedulerRemote);
            SchedulerInfo.Add("Scheduler Type", metadata.SchedulerType);
            SchedulerInfo.Add("JobStore Type", metadata.JobStoreType);
            SchedulerInfo.Add("Support Persistence", metadata.JobStoreSupportsPersistence);
            SchedulerInfo.Add("Clustered", metadata.JobStoreClustered);
            SchedulerInfo.Add("Thread Pool Size", metadata.ThreadPoolSize);
            SchedulerInfo.Add("Thread Pool Type", metadata.ThreadPoolType);
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

        public void Dispose()
        {
            _refreshTimer?.Dispose();
            UnRegisterEventListeners();
        }

        #region Action buttons
        private async Task OnStartScheduler()
        {
            await SchSvc.StartScheduler();
        }

        private async Task OnStandbyScheduler()
        {
            await SchSvc.StandbyScheduler();
        }

        private async Task OnShutdownScheduler()
        {
            await SchSvc.ShutdownScheduler();
        }

        private async Task OnPauseAllSchedules()
        {
            try
            {
                await SchSvc.PauseAllSchedules();
                Snackbar.Add("Paused all schedules", Severity.Info);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error pausing all schedules. {ex.Message}", Severity.Error);
            }
        }

        private async Task OnResumeAllSchedules()
        {
            try
            {
                await SchSvc.ResumeAllSchedules();
                Snackbar.Add("Resumed all schedules", Severity.Info);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error resuming all schedules. {ex.Message}", Severity.Error);
            }
        }
        #endregion Action buttons

        #region Auto refresh
        private void StartAutoRefresh()
        {
            _refreshTimer?.Change(0, REFRESH_IN_MS);
        }

        private void StopAutoRefresh()
        {
            _refreshTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void OnCheckAutoRefresh(bool flag)
        {
            AutoRefresh = flag;
            if (AutoRefresh)
            {
                StartAutoRefresh();
            }
            else
            {
                StopAutoRefresh();
            }
        }
        #endregion Auto refresh
    }
}

