using System;
namespace BlazingQuartz.Core.History
{
    public class PostgreSQLExecutionLogRawSqlProvider : IExecutionLogRawSqlProvider
    {
        public string DeleteLogsByDays => @"DELETE FROM ""bqz_ExecutionLogs""
WHERE ""DateAddedUtc"" < {0}";
    }
}

