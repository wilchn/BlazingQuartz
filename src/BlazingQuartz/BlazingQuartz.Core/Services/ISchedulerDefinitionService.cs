using System;
using Quartz;

namespace BlazingQuartz.Core.Services
{
	public interface ISchedulerDefinitionService
	{
		IEnumerable<IntervalUnit> GetTriggerIntervalUnits(TriggerType triggerType);
		IEnumerable<MisfireAction> GetMisfireActions(TriggerType triggerType);
	}
}

