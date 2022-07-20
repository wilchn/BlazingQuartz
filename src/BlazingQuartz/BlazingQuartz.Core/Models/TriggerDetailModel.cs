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
        public DateTimeOffset? StartTime { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public string? CalendarName { get; set; }
        public string? Timezone { get; set; }
        public int Priority { get; set; } = 5;
        public string? CronExpression { get; set; }
        public bool RepeatForever { get; set; }

        public IntervalUnit? TriggerIntervalUnit { get; set; }
        public MisfireAction MisfireAction { get; set; }
    }
}

