using System;
namespace BlazingQuartz.Core.Models
{
    public class ScheduleModel
    {
        public string? JobName { get; set; }
        public string? JobType { get; set; }
        public string? JobDescription { get; set; }
        public string JobGroup { get; set; } = "No Group";
        public string? TriggerName { get; set; }
        public string? TriggerGroup { get; set; }
        public string? TriggerDescription { get; set; }
        public TriggerType TriggerType { get; set; }
        public string? TriggerTypeClassName { get; set; }
        public JobStatus JobStatus { get; set; } = JobStatus.Idle;

        public DateTimeOffset? NextTriggerTime { get; set; }
        public DateTimeOffset? PreviousTriggerTime { get; set; }
    }
}

