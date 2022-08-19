using System;
using BlazingQuartz.Core.Data;
using BlazingQuartz.Core.Data.Entities;
using Microsoft.Extensions.Logging;

namespace BlazingQuartz.Core.History
{
    public class ExecutionLogStore : IExecutionLogStore
    {
        private readonly ILogger<ExecutionLogStore> _logger;
        private readonly BlazingQuartzDbContext _dbContext;

        public ExecutionLogStore(ILogger<ExecutionLogStore> logger,
            BlazingQuartzDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task AddExecutionLog(ExecutionLog log, CancellationToken cancelToken = default)
        {
            await _dbContext.ExecutionLogs.AddAsync(log, cancelToken);
        }

        public bool Exists(ExecutionLog log)
        {
            return _dbContext.ExecutionLogs.Any(l => l.RunInstanceId == log.RunInstanceId);
        }

        public async Task SaveChangesAsync(CancellationToken cancelToken = default)
        {
            await _dbContext.SaveChangesAsync(cancelToken);
        }

        public ValueTask UpdateExecutionLog(ExecutionLog log)
        {
            var entry = _dbContext.ExecutionLogs.Where(l => l.RunInstanceId == log.RunInstanceId)
                .FirstOrDefault();

            if (entry != null)
            {
                entry.ExceptionMessage = log.ExceptionMessage;
                entry.ExecutionDetails = log.ExecutionDetails;
                entry.IsVetoed = log.IsVetoed;
                entry.JobRunTime = log.JobRunTime;
                entry.Result = log.Result;
                entry.IsException = log.IsException;

                _dbContext.ExecutionLogs.Update(entry);
            }
            else
            {
                _logger.LogWarning("Failed to UpdateExecutionLog. Cannot find run instance id [{runInstanceId}]",
                    log.RunInstanceId);
            }

            return ValueTask.CompletedTask;
        }
    }
}

