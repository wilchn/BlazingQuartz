using System;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using Quartz.Spi;

namespace BlazingQuartz.Test.Shared
{
    public class TestUtils
    {
        public static TriggerFiredBundle NewMinimalTriggerFiredBundle()
        {
            return NewMinimalTriggerFiredBundle(false);
        }

        private static TriggerFiredBundle NewMinimalTriggerFiredBundle(bool isRecovering)
        {
            IJobDetail jd = JobBuilder.Create<TestJob>()
                                      .WithIdentity(new JobKey("jobName", "jobGroup"))
                                      .Build();
            IOperableTrigger trigger = new SimpleTriggerImpl("triggerName", "triggerGroup");
            TriggerFiredBundle retValue = new TriggerFiredBundle(jd, trigger, null, isRecovering, DateTimeOffset.UtcNow, null, null, null);

            return retValue;
        }

        public static IJobExecutionContext NewJobExecutionContextFor(IJob job)
        {
            return new JobExecutionContextImpl(null!, NewMinimalTriggerFiredBundle(), job);
        }
    }
}

