using System;
namespace BlazingQuartz.Core.History
{
    public class BaseExecutionLogRawSqlProvider : IExecutionLogRawSqlProvider
    {
        public virtual string DeleteLogsByDays { get; } =
            @"DELETE FROM bqz_ExecutionLogs
WHERE DateAddedUtc < '{0}'";
    }
}

