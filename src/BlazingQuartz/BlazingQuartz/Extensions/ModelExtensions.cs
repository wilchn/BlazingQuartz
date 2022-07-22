using System;
using BlazingQuartz.Core;
using MudBlazor;

namespace BlazingQuartz.Extensions
{
	public static class ModelExtensions
	{
		public static string GetTriggerTypeIcon(this TriggerType triggerType)
		{
			switch (triggerType)
			{
				case TriggerType.Cron:
					return Icons.Filled.Schedule;
				case TriggerType.Daily:
					return Icons.Filled.Alarm;
				case TriggerType.Simple:
					return Icons.Filled.Repeat;
				case TriggerType.Calendar:
					return Icons.Filled.CalendarMonth;
				default:
					return Icons.Filled.Settings;
			}
		}
	}
}

