using System;
using BlazingQuartz.Core.Data.Entities;

namespace BlazingQuartz.Core.History
{
    public interface IExecutionLogStore
    {
        bool Exists(ExecutionLog log);
        Task AddExecutionLog(ExecutionLog log, CancellationToken cancelToken = default);
        ValueTask UpdateExecutionLog(ExecutionLog log);
        Task SaveChangesAsync(CancellationToken cancelToken = default);
    }
}

