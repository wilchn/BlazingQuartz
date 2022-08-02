using System;
using Quartz;

namespace BlazingQuartz.Core.Models
{
    public class TriggerDetailModel
    {
        public string Name { get; set; } = string.Empty;
        public string Group { get; set; } = "No Group";
        public TriggerType TriggerType { get; set; }
        public string? Description { get; set; }
        public TimeSpan? StartTimeSpan { get; set; }
        public DateTime? StartDate { get; set; }
        public TimeSpan? EndTimeSpan { get; set; }
        public DateTime? EndDate { get; set; }
        
        public string? ModifiedByCalendar { get; set; }
        /// <summary>
        /// Timezone of start time
        /// </summary>
        public TimeZoneInfo StartTimezone { get; set; } = TimeZoneInfo.Utc;
        public int Priority { get; set; } = 5;
        public string? CronExpression { get; set; }
        public bool RepeatForever { get; set; }
        public int RepeatCount { get; set; }

        public bool[] DailyDayOfWeek { get; set; } = new bool[7];
        public TimeSpan? StartDailyTime { get; set; }
        public TimeSpan? EndDailyTime { get; set; }
        /// <summary>
        /// The timezone in which to base the scheduled. Used in Cron schedule, Calendar schedule and Daily schedule.
        /// </summary>
        public TimeZoneInfo InTimeZone { get; set; } = TimeZoneInfo.Local;

        public int TriggerInterval { get; set; } = 1;
        public IntervalUnit? TriggerIntervalUnit { get; set; } = IntervalUnit.Minute;
        public MisfireAction MisfireAction { get; set; } = MisfireAction.SmartPolicy;

        public IDictionary<string, object> TriggerDataMap { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyCollection<DayOfWeek> GetDailyOnDaysOfWeek()
        {
            var dayOfWeekCount = 7;
            var list = new List<DayOfWeek>(dayOfWeekCount);
            for (int i = 0; i < dayOfWeekCount; i++)
            {
                if (DailyDayOfWeek[i])
                {
                    list.Add((DayOfWeek)Enum.ToObject(typeof(DayOfWeek), i));
                }
            }

            return list;
        }
    }
}

