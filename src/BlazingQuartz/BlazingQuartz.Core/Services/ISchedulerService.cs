using BlazingQuartz.Core.Models;
using Quartz;

namespace BlazingQuartz.Core.Services
{
    public interface ISchedulerService
    {
        //ScheduleModel CreateScheduleModel(IJobDetail? jobDetail, ITrigger trigger);
        Task<ScheduleModel> GetScheduleModelAsync(ITrigger trigger);
        IAsyncEnumerable<ScheduleModel> GetAllJobsAsync();
        Task CreateSchedule(ScheduleModel model);
    }
}