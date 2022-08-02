using BlazingQuartz.Core.Models;

namespace BlazingQuartz.Models
{
    public interface ITriggerDetailModelValidator
    {
        bool ValidateDaysOfWeek(TriggerDetailModel triggerModel);
        Task<string?> ValidateTriggerName(string name, TriggerDetailModel triggerModel);
        string? ValidateTime(TimeSpan? start, TimeSpan? end, string errorMessage);
        string? ValidateFirstLastDateTime(TriggerDetailModel model, string errorMessage);
    }
}