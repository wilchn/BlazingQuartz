using System;
using BlazingQuartz.Core;
using BlazingQuartz.Core.Models;
using BlazingQuartz.Core.Services;
using Microsoft.AspNetCore.Components;
using BlazingQuartz.Extensions;
using MudBlazor;
using BlazingQuartz.Models;

namespace BlazingQuartz.Components
{
    public partial class BlazingTrigger : ComponentBase
    {
		[Inject] private ISchedulerDefinitionService SchedulerDefSvc { get; set; } = null!;
        [Inject] private ISchedulerService SchedulerSvc { get; set; } = null!;
        [Inject] private ITriggerDetailModelValidator Validator { get; set; } = null!;
        [Inject] private IDialogService DialogSvc { get; set; } = null!;

        [Parameter]
		[EditorRequired]
		public TriggerDetailModel TriggerDetail { get; set; } = new();

        [Parameter] public bool IsValid { get; set; }

		[Parameter] public EventCallback<bool> IsValidChanged { get; set; }

		private ISet<TriggerType> ExcludedTriggerTypeChoices = new HashSet<TriggerType> { TriggerType.Unknown, TriggerType.Calendar };

        private IEnumerable<string>? ExistingTriggerGroups;

		private string? CronDescription;
		private MudForm _form = null!;
        private bool _isDaysOfWeekValid = true;
        private IReadOnlyCollection<string>? _calendars;
        private MudTimePicker _endDailyTimePicker = null!;
        private MudDatePicker _endDatePicker = null!;
        private Key? OriginalTriggerKey { get; set; }

		private Dictionary<TriggerType, string> TriggerTypeIcons = new()
		{
			{ TriggerType.Cron, TriggerType.Cron.GetTriggerTypeIcon() },
			{ TriggerType.Daily, TriggerType.Daily.GetTriggerTypeIcon() },
			{ TriggerType.Simple, TriggerType.Simple.GetTriggerTypeIcon() },
			{ TriggerType.Calendar, TriggerType.Calendar.GetTriggerTypeIcon() },
		};

		protected override void OnInitialized()
		{
            OriginalTriggerKey = new(TriggerDetail.Name, TriggerDetail.Group);
        }

		private void OnCronExpressionInputElapsed(string? cronExpression)
		{
			try
			{
				CronDescription = CronExpressionDescriptor.ExpressionDescriptor.GetDescription(cronExpression);
			}
			catch
			{
				CronDescription = "Check cron expression";
			}
		}

        async Task<IEnumerable<string>> SearchTriggerGroup(string value)
        {
            if (ExistingTriggerGroups == null)
            {
                ExistingTriggerGroups = await SchedulerSvc.GetTriggerGroups();
            }
            
            return ExistingTriggerGroups.Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase));
        }

        async Task<IEnumerable<TimeZoneInfo>> SearchTimeZoneInfo(string value)
        {
			await Task.CompletedTask;

            var tzList = TimeZoneInfo.GetSystemTimeZones();

			if (string.IsNullOrEmpty(value))
			{
				return tzList;
			}
            
            return tzList.Where(x => x.DisplayName.Contains(value, StringComparison.InvariantCultureIgnoreCase));
        }

        async Task<IEnumerable<string>> SearchCalendars(string value)
        {
            if (_calendars == null)
            {
                _calendars = await SchedulerSvc.GetCalendarNames();
            }

            return _calendars.Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase));
        }

        private void OnSetIsValid(bool value)
        {
            if (IsValid == value)
                return;
            IsValid = value;
            IsValidChanged.InvokeAsync(value).AndForget();
        }

		public async Task Validate()
		{
            await _form.Validate();

            _isDaysOfWeekValid = Validator.ValidateDaysOfWeek(TriggerDetail);
            if (_form.IsValid)
                OnSetIsValid(_isDaysOfWeekValid);
		}

        async Task OnStartDailyTimeChanged(TimeSpan? time)
        {
            TriggerDetail.StartDailyTime = time;
            await _endDailyTimePicker.Validate();
        }

        async Task OnStartTimeChanged(TimeSpan? time)
        {
            TriggerDetail.StartTimeSpan = time;
            await _endDatePicker.Validate();
        }

        async Task OnStartDateChanged(DateTime? time)
        {
            TriggerDetail.StartDate = time;
            await _endDatePicker.Validate();
        }

        async Task OnAddDataMap()
        {
            var options = new DialogOptions {
                CloseOnEscapeKey = true,
                DisableBackdropClick = true,
                FullWidth = true,
                MaxWidth = MaxWidth.Small
            };
            var parameters = new DialogParameters { 
                ["ExistingDataMap"] = new Dictionary<string, object>(TriggerDetail.TriggerDataMap, 
                    StringComparer.OrdinalIgnoreCase)
            };

            var dialog = DialogSvc.Show<JobDataMapDialog>("Add Data Map", parameters, options);
            var result = await dialog.Result;

            if (!result.Cancelled)
            {
                var dataMap = (DataMapItemModel)result.Data;
                if (dataMap.Key != null && dataMap.Value != null)
                    TriggerDetail.TriggerDataMap.Add(dataMap.Key, dataMap.Value);
                else
                {
                    // TODO print error message. Data map is null
                }
            }
        }

        async Task OnEditDataMap(KeyValuePair<string, object> item)
        {
            var options = new DialogOptions
            {
                CloseOnEscapeKey = true,
                DisableBackdropClick = true,
                FullWidth = true,
                MaxWidth = MaxWidth.Small
            };
            var parameters = new DialogParameters
            {
                ["ExistingDataMap"] = TriggerDetail.TriggerDataMap,
                ["DataMapItem"] = new DataMapItemModel(item),
                ["IsEditMode"] = true
            };

            var dialog = DialogSvc.Show<JobDataMapDialog>("Edit Data Map", parameters, options);
            var result = await dialog.Result;

            if (!result.Cancelled)
            {
                var dataMap = (DataMapItemModel)result.Data;
                if (dataMap.Key != null && dataMap.Value != null)
                {
                    TriggerDetail.TriggerDataMap[dataMap.Key] = dataMap.Value;
                }
                else
                {
                    // TODO print error message. Data map is null
                }
            }
        }

        async Task OnCloneDataMap(KeyValuePair<string, object> item)
        {
            var options = new DialogOptions
            {
                CloseOnEscapeKey = true,
                DisableBackdropClick = true,
                FullWidth = true,
                MaxWidth = MaxWidth.Small
            };
            int index = 1;
            var key = item.Key + index++;
            
            while (TriggerDetail.TriggerDataMap.ContainsKey(key))
            {
                if (index == int.MaxValue)
                {
                    key = string.Empty;
                    break;
                }
                    
                key = item.Key + index++;
            }
            var clonedItem = new KeyValuePair<string, object>(key, item.Value);
            var parameters = new DialogParameters
            {
                ["ExistingDataMap"] = new Dictionary<string, object>(TriggerDetail.TriggerDataMap, 
                    StringComparer.OrdinalIgnoreCase),
                ["DataMapItem"] = new DataMapItemModel(clonedItem)
            };

            var dialog = DialogSvc.Show<JobDataMapDialog>("Add Data Map", parameters, options);
            var result = await dialog.Result;

            if (!result.Cancelled)
            {
                var dataMap = (DataMapItemModel)result.Data;
                if (dataMap.Key != null && dataMap.Value != null)
                {
                    TriggerDetail.TriggerDataMap[dataMap.Key] = dataMap.Value;
                }
                else
                {
                    // TODO print error message. Data map is null
                }
            }
        }

        async Task OnDeleteDataMap(KeyValuePair<string, object> item)
        {
            bool? yes = await DialogSvc.ShowMessageBox(
                "Confirm Delete", 
                $"Do you want to delete '{item.Key}'?", 
                yesText:"Yes", cancelText:"No");

            if (yes == null || !yes.Value)
            {
                return;
            }

            TriggerDetail.TriggerDataMap.Remove(item);
        }
	}
}

