using System;
using Quartz;

namespace BlazingQuartz.Core.Models
{
    public class TriggerDetailModel
    {
        public string Name { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public TriggerType TriggerType { get; set; }
        public string? Description { get; set; }
        public TimeSpan? StartTimeSpan { get; set; }
        public DateTime? StartDate { get; set; }
        public TimeSpan? EndTimeSpan { get; set; }
        public DateTime? EndDate { get; set; }
        
        public string? CalendarName { get; set; }
        public TimeZoneInfo Timezone { get; set; } = TimeZoneInfo.Utc;
        public int Priority { get; set; } = 5;
        public string? CronExpression { get; set; }
        public bool RepeatForever { get; set; }

        public bool[] DailyDayOfWeek { get; set; } = new bool[7];
        public TimeSpan? StartDailyTime { get; set; }
        public TimeSpan? EndDailyTime { get; set; }
        public TimeZoneInfo DailyTimeZone { get; set; } = TimeZoneInfo.Utc;

        public int TriggerInterval { get; set; }
        public IntervalUnit? TriggerIntervalUnit { get; set; }
        public MisfireAction MisfireAction { get; set; } = MisfireAction.SmartPolicy;
    }
}

