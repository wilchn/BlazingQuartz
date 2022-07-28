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
    }
}