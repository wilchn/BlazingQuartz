using BlazingQuartz.Core.Models;
using Quartz;

namespace BlazingQuartz.Core.Services
{
    public interface ISchedulerService
    {
        Task<ScheduleModel> GetScheduleModelAsync(ITrigger trigger);
        IAsyncEnumerable<ScheduleModel> GetAllJobsAsync();
        Task CreateSchedule(JobDetailModel jobDetailModel, TriggerDetailModel triggerDetailModel);
        Task<IReadOnlyCollection<string>> GetJobGroups();
        Task<IReadOnlyCollection<string>> GetTriggerGroups();
        Task<JobDetailModel?> GetJobDetail(string jobName, string groupName);
        Task<TriggerDetailModel?> GetTriggerDetail(string triggerName, string triggerGroup);
        Task<bool> ContainsTriggerKey(string triggerName, string triggerGroup);
        Task<bool> ContainsJobKey(string jobName, string jobGroup);
        Task<IReadOnlyCollection<string>> GetCalendarNames(CancellationToken cancelToken = default);
    }
}