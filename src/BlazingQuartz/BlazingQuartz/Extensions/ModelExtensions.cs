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

		public static DataMapType GetDataMapType(this KeyValuePair<string, object> kv)
		{
			/*
			bool	System.Boolean
			byte	System.Byte
			sbyte	System.SByte
			char	System.Char
			decimal	System.Decimal
			double	System.Double
			float	System.Single
			int	System.Int32
			uint	System.UInt32
			nint	System.IntPtr
			nuint	System.UIntPtr
			long	System.Int64
			ulong	System.UInt64
			short	System.Int16
			ushort	System.UInt16
			*/
            switch(kv.Value.GetType().FullName)
            {
                case "System.String":
                    return DataMapType.String;
                case "System.Int32":
                    return DataMapType.Integer;
				case "System.Int64":
					return DataMapType.Long;
				case "System.Boolean":
					return DataMapType.Bool;
				case "System.Single":
					return DataMapType.Float;
				case "System.Decimal":
					return DataMapType.Decimal;
				case "System.Double":
					return DataMapType.Double;
				case "System.Int16":
					return DataMapType.Short;
				case "System.Char":
					return DataMapType.Char;
            }
            return DataMapType.Object;
		}

		public static string GetDataMapTypeDescription(this KeyValuePair<string, object> kv)
		{
			var mapType = kv.GetDataMapType();
			if (mapType == DataMapType.Object)
			{
				return $"Object ({kv.Value.GetType().FullName})";
			}
			return mapType.ToString();
		}

		/// <summary>
		/// Converts <see cref="TimeSpan"/> objects to a simple human-readable string.  Examples: 3.1 seconds, 2 minutes, 4.23 hours, etc.
		/// </summary>
		/// <param name="span">The timespan.</param>
		/// <param name="significantDigits">Significant digits to use for output.</param>
		/// <returns></returns>
		public static string ToHumanTimeString(this TimeSpan span, int significantDigits = 3)
		{
			var format = "G" + significantDigits;
			return span.TotalMilliseconds < 1000 ? span.TotalMilliseconds.ToString(format) + " ms"
				: (span.TotalSeconds < 60 ? span.TotalSeconds.ToString(format) + (span.TotalSeconds == 1 ? " sec" : " secs")
					: (span.TotalMinutes < 60 ? span.TotalMinutes.ToString(format) + (span.TotalMinutes == 1 ? " min" : " mins")
						: (span.TotalHours < 24 ? span.TotalHours.ToString(format) + (span.TotalHours == 1 ? " hr" : " hrs")
												: span.TotalDays.ToString(format) + (span.TotalDays == 1 ? " day" : " days"))));
		}
	}
}

