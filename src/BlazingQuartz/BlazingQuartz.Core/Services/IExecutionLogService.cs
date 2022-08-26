using BlazingQuartz.Core.Data.Entities;
using BlazingQuartz.Core.Models;

namespace BlazingQuartz.Core.Services
{
    public interface IExecutionLogService
    {
        Task<PagedList<ExecutionLog>> GetLatestExecutionLog(string jobName, string jobGroup,
            string? triggerName, string? triggerGroup,
            PageMetadata? pageMetadata = null, long firstLogId = 0);
        Task<PagedList<ExecutionLog>> GetExecutionLogs(
            ExecutionLogFilter? filter = null,
            PageMetadata? pageMetadata = null, long firstLogId = 0);
        Task<IList<string>> GetJobNames();
        Task<IList<string>> GetJobGroups();
        Task<IList<string>> GetTriggerNames();
        Task<IList<string>> GetTriggerGroups();
    }
}