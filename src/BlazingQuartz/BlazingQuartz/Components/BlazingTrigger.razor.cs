using System;
using BlazingQuartz.Core;
using BlazingQuartz.Core.Models;
using BlazingQuartz.Core.Services;
using Microsoft.AspNetCore.Components;
using BlazingQuartz.Extensions;
using MudBlazor;

namespace BlazingQuartz.Components
{
    public partial class BlazingTrigger : ComponentBase
    {
		[Inject]
		private ISchedulerDefinitionService SchedulerDefSvc { get; set; } = null!;
        [Inject]
        private ISchedulerService SchedulerSvc { get; set; } = null!;

		[Parameter]
		[EditorRequired]
		public TriggerDetailModel TriggerDetail { get; set; } = new();

        [Parameter] public bool IsValid { get; set; }

		[Parameter] public EventCallback<bool> IsValidChanged { get; set; }

		private ISet<TriggerType> ExcludedTriggerTypeChoices = new HashSet<TriggerType> { TriggerType.Unknown };

        private IEnumerable<string>? ExistingTriggerGroups;

		private string? CronDescription;
		private MudForm _form = null!;

		private Dictionary<TriggerType, string> TriggerTypeIcons = new()
		{
			{ TriggerType.Cron, TriggerType.Cron.GetTriggerTypeIcon() },
			{ TriggerType.Daily, TriggerType.Daily.GetTriggerTypeIcon() },
			{ TriggerType.Simple, TriggerType.Simple.GetTriggerTypeIcon() },
			{ TriggerType.Calendar, TriggerType.Calendar.GetTriggerTypeIcon() },
		};

		protected override void OnInitialized()
		{
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

        private void OnSetIsValid(bool value)
        {
            if (IsValid == value)
                return;
            IsValid = value;
            IsValidChanged.InvokeAsync(value).AndForget();
        }

		public Task Validate()
		{
			return _form.Validate();
		}

	}
}

