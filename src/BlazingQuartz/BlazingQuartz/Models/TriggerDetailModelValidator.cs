using System;
using BlazingQuartz.Core;
using BlazingQuartz.Core.Models;
using BlazingQuartz.Core.Services;

namespace BlazingQuartz.Models
{
    public class TriggerDetailModelValidator : ITriggerDetailModelValidator
    {
        private readonly ISchedulerService _schedulerSvc;

        public TriggerDetailModelValidator(ISchedulerService schSvc)
        {
            _schedulerSvc = schSvc;
        }

        /// <summary>
        /// Returns true if Days of week validation is successful
        /// </summary>
        /// <param name="triggerModel"></param>
        /// <returns></returns>
        public bool ValidateDaysOfWeek(TriggerDetailModel triggerModel)
        {
            if (triggerModel.TriggerType != TriggerType.Daily)
                return true;

            foreach (var val in triggerModel.DailyDayOfWeek)
            {
                if (val)
                {
                    return true;
                }
            }

            return false;
        }


        public async Task<string?> ValidateTriggerName(string name, TriggerDetailModel triggerModel,
            Key? originalKey)
        {
            if (string.IsNullOrEmpty(name))
                return "Trigger name is required";

            if (originalKey != null &&
                originalKey.Equals(name, triggerModel.Group))
                return null;

            var exists = await _schedulerSvc.ContainsTriggerKey(name, triggerModel.Group);

            if (exists)
                return "Trigger name and group already defined. Please choose another name.";

            return null;
        }

        public string? ValidateTime(TimeSpan? start, TimeSpan? end, string errorMessage)
        {
            if (start.HasValue && end.HasValue)
            {
                if (start.Value > end.Value)
                    return errorMessage;
            }

            return null;
        }

        public string? ValidateFirstLastDateTime(TriggerDetailModel model, string errorMessage)
        {
            if (!model.StartDate.HasValue ||
                !model.EndDate.HasValue)
                return null;

            var start = model.StartDate.Value.Add(model.StartTimeSpan ?? TimeSpan.Zero);
            var end = model.EndDate.Value.Add(model.EndTimeSpan ?? TimeSpan.Zero);

            if (start > end)
                return errorMessage;

            return null;
        }
    }
}

