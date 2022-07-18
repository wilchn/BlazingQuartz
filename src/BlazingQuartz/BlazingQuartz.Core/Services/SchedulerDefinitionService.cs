using System;
using System.Diagnostics.CodeAnalysis;
using Quartz;

namespace BlazingQuartz.Core.Services
{
    internal class SchedulerDefinitionService : ISchedulerDefinitionService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private IEnumerable<IntervalUnit> _calendarIntervalUnits;
        private IEnumerable<IntervalUnit> _simpleIntervalUnits;
        private IEnumerable<MisfireAction>? _cronCalDailyMisfireActions;
        private IEnumerable<MisfireAction>? _simpleMisfireActions;

        public SchedulerDefinitionService(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
            Init();
        }

        [MemberNotNull(nameof(_calendarIntervalUnits))]
        [MemberNotNull(nameof(_simpleIntervalUnits))]
        private void Init()
        {
            _calendarIntervalUnits = new List<IntervalUnit>
            {
                IntervalUnit.Second,
                IntervalUnit.Minute,
                IntervalUnit.Hour,
                IntervalUnit.Day,
                IntervalUnit.Week,
                IntervalUnit.Month,
                IntervalUnit.Year
            };
            _simpleIntervalUnits = new List<IntervalUnit>
            {
                IntervalUnit.Second,
                IntervalUnit.Minute,
                IntervalUnit.Hour,
            };
        }

        public IEnumerable<IntervalUnit> GetTriggerIntervalUnits(TriggerType triggerType)
        {
            switch (triggerType)
            {
                case TriggerType.Calendar:
                    return _calendarIntervalUnits;
                case TriggerType.Daily:
                case TriggerType.Simple:
                    return _simpleIntervalUnits;
                default:
                    return Enumerable.Empty<IntervalUnit>();
            }
        }

        public IEnumerable<MisfireAction> GetMisfireActions(TriggerType triggerType)
        {
            switch (triggerType)
            {
                case TriggerType.Cron:
                case TriggerType.Daily:
                case TriggerType.Calendar:
                    if (_cronCalDailyMisfireActions == null)
                    {
                        _cronCalDailyMisfireActions = new List<MisfireAction>
                        {
                            MisfireAction.SmartPolicy,
                            MisfireAction.DoNothing,
                            MisfireAction.IgnoreMisfirePolicy,
                            MisfireAction.FireOnceNow
                        };
                    }
                    return _cronCalDailyMisfireActions;
                case TriggerType.Simple:
                    if (_simpleMisfireActions == null)
                    {
                        _simpleMisfireActions = new List<MisfireAction>
                        {
                            MisfireAction.SmartPolicy,
                            MisfireAction.FireNow,
                            MisfireAction.IgnoreMisfirePolicy,
                            MisfireAction.RescheduleNextWithExistingCount,
                            MisfireAction.RescheduleNextWithRemainingCount,
                            MisfireAction.RescheduleNowWithExistingRepeatCount,
                            MisfireAction.RescheduleNowWithRemainingRepeatCount
                        };
                    }
                    return _simpleMisfireActions;
            }

            return Enumerable.Empty<MisfireAction>();
        }
    }
}

