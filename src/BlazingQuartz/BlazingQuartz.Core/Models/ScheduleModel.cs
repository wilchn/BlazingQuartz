using System;
namespace BlazingQuartz.Core.Models
{
    public class ScheduleModel
    {
        public string? JobName { get; set; }
        public string? JobType { get; set; }
        public string? JobDescription { get; set; }
        public string JobGroup { get; set; } = Constants.DEFAULT_GROUP;
        public string? TriggerName { get; set; }
        public string? TriggerGroup { get; set; }
        public string? TriggerDescription { get; set; }
        public TriggerDetailModel? TriggerDetail { get; set; }
        public TriggerType TriggerType { get; set; }
        public string? TriggerTypeClassName { get; set; }
        public JobStatus JobStatus { get; set; } = JobStatus.Idle;

        public DateTimeOffset? NextTriggerTime { get; set; }
        public DateTimeOffset? PreviousTriggerTime { get; set; }
        public string? ExceptionMessage { get; set; }

        public void ClearTrigger()
        {
            TriggerName = null;
            TriggerGroup = null;
            TriggerDescription = null;
            TriggerDetail = null;
            TriggerTypeClassName = null;
            NextTriggerTime = null;
            PreviousTriggerTime = null;
            TriggerType = TriggerType.Unknown;
        }
    }
}

