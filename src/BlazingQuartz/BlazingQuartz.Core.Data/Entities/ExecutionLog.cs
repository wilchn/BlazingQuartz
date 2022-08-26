using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazingQuartz.Core.Data.Entities
{
    public class ExecutionLog
    {
        [Key]
        public long LogId { get; set; }
        [MaxLength(256)]
        public string? RunInstanceId { get; set; }
        [Column(TypeName = "varchar(20)")]
        public LogType LogType { get; set; }
        [MaxLength(256)]
        public string? JobName { get; set; }
        [MaxLength(256)]
        public string? JobGroup { get; set; }
        [MaxLength(256)]
        public string? TriggerName { get; set; }
        [MaxLength(256)]
        public string? TriggerGroup { get; set; }
        public DateTimeOffset? ScheduleFireTimeUtc { get; set; }
        public DateTimeOffset? FireTimeUtc { get; set; }
        public TimeSpan? JobRunTime { get; set; }
        public int? RetryCount { get; set; }
        [MaxLength(8000)]
        public string? Result { get; set; }
        public ExceptionMessage? ExceptionMessage { get; set; }
        public string? ExecutionDetails { get; set; }
        public bool? IsVetoed { get; set; }
        public bool? IsException { get; set; }
        public DateTimeOffset DateAddedUtc { get; set; }

        public ExecutionLog()
        {
            DateAddedUtc = DateTimeOffset.UtcNow;
        }

        public DateTimeOffset? GetFinishTimeUtc() => FireTimeUtc?.Add(JobRunTime ?? TimeSpan.Zero);
    }
}

