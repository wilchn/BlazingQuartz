using System;
namespace BlazingQuartz.Core.Data.Entities
{
    public class ExceptionMessage
    {
        public string Message { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public int? ErrorCode { get; set; }
        public string? HelpLink { get; set; }
    }
}

