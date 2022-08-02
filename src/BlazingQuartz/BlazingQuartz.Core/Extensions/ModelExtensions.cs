using System;
using BlazingQuartz.Core.Models;
using Quartz;

namespace BlazingQuartz.Core
{
	public static class ModelExtensions
	{
		public static bool Equal(this ScheduleModel model, JobKey jobKey, TriggerKey triggerKey)
		{
			return model.JobName == jobKey.Name &&
				model.JobGroup == jobKey.Group &&
				model.TriggerName == triggerKey.Name &&
				model.TriggerGroup == triggerKey.Group;
		}

		public static TriggerType GetTriggerType(this ITrigger trigger)
		{
			if (trigger is ICronTrigger)
				return TriggerType.Cron;
			if (trigger is ISimpleTrigger)
				return TriggerType.Simple;
			if (trigger is ICalendarIntervalTrigger)
				return TriggerType.Calendar;
			if (trigger is IDailyTimeIntervalTrigger)
				return TriggerType.Daily;

			return TriggerType.Unknown;
		}

		public static TimeOfDay ToTimeOfDay(this TimeSpan timeSpan)
        {
			return new TimeOfDay(timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }

		public static Quartz.IntervalUnit ToQuartzIntervalUnit(this IntervalUnit value)
        {
			return Enum.Parse<Quartz.IntervalUnit>(value.ToString());
        }

		public static IntervalUnit ToBlazingQuartzIntervalUnit(this Quartz.IntervalUnit value)
        {
			return Enum.Parse<IntervalUnit>(value.ToString());
		}
	}
}

