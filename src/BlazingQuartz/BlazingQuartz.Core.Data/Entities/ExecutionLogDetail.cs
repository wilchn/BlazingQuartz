using System;
using System.ComponentModel.DataAnnotations;

namespace BlazingQuartz.Core.Data.Entities
{
    public class ExecutionLogDetail
    {
        public string? ExecutionDetails { get; set; }
        public string? ErrorStackTrace { get; set; }
        public int? ErrorCode { get; set; }
        [MaxLength(1000)]
        public string? ErrorHelpLink { get; set; }
    }
}

