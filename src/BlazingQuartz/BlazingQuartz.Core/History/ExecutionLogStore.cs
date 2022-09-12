using System;
using Microsoft.EntityFrameworkCore;
using BlazingQuartz.Core.Data;
using BlazingQuartz.Core.Data.Entities;
using Microsoft.Extensions.Logging;

namespace BlazingQuartz.Core.History
{
    public class ExecutionLogStore : IExecutionLogStore
    {
        private readonly ILogger<ExecutionLogStore> _logger;
        private readonly BlazingQuartzDbContext _dbContext;
        private readonly IExecutionLogRawSqlProvider _sqlProvider;

        public ExecutionLogStore(ILogger<ExecutionLogStore> logger,
            BlazingQuartzDbContext dbContext,
            IExecutionLogRawSqlProvider sqlProvider)
        {
            _logger = logger;
            _dbContext = dbContext;
            _sqlProvider = sqlProvider;
        }

        public async Task AddExecutionLog(ExecutionLog log, CancellationToken cancelToken = default)
        {
            await _dbContext.ExecutionLogs.AddAsync(log, cancelToken);
        }

        public bool Exists(ExecutionLog log)
        {
            return _dbContext.ExecutionLogs.Any(l => l.RunInstanceId == log.RunInstanceId);
        }

        public async Task<int> DeleteLogsByDays(int daysToKeep, CancellationToken cancelToken = default)
        {
            DateTime oldDate = DateTime.UtcNow.Date.AddDays(-(daysToKeep+1));

            IEnumerable<object> parameters = new List<object>
            {
                oldDate
            };
            return await _dbContext.Database.ExecuteSqlRawAsync(_sqlProvider.DeleteLogsByDays,
                parameters, cancelToken);
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
                entry.ExecutionLogDetail = log.ExecutionLogDetail;
                entry.ErrorMessage = log.ErrorMessage;
                entry.ExecutionLogDetail = log.ExecutionLogDetail;
                entry.IsVetoed = log.IsVetoed;
                entry.JobRunTime = log.JobRunTime;
                entry.Result = log.Result;
                entry.IsException = log.IsException;
                entry.IsSuccess = log.IsSuccess;
                entry.ReturnCode = log.ReturnCode;

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

