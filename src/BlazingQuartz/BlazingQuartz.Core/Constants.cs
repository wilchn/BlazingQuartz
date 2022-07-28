using System;
using System.ComponentModel;

namespace BlazingQuartz.Core
{
    public enum JobStatus
    {
        Running,
        Idle,
        Paused,
        NoTrigger
    }

    public enum TriggerType
    {
        Cron,
        Daily,
        Simple,
        Calendar,
        Unknown
    }

    public enum MisfireAction
    {
        [Description("Instructs the IScheduler that the ITrigger will never be evaluated for a misfire situation, and that the scheduler will simply try to fire it as soon as it can, and then update the Trigger as if it had fired at the proper time.")]
        IgnoreMisfirePolicy,
        [Description("Instruction not set (yet).")]
        InstructionNotSet,
        [Description("Use smart policy.")]
        SmartPolicy,
        [Description("Fired now by IScheduler. NOTE: This instruction should typically only be used for 'one-shot' (non-repeating) Triggers. If it is used on a trigger with a repeat count > 0 then it is equivalent to the instruction RescheduleNowWithRemainingRepeatCount.")]
        FireNow,
        [Description("Re-scheduled to the next scheduled time after 'now' - taking into account any associated ICalendar, and with the repeat count left unchanged.")]
        RescheduleNextWithExistingCount,
        [Description("Re-scheduled to the next scheduled time after 'now' - taking into account any associated ICalendar, and with the repeat count set to what it would be, if it had not missed any firings.")]
        RescheduleNextWithRemainingCount,
        [Description("Re-scheduled to 'now' (even if the associated ICalendar excludes 'now') with the repeat count left as-is. This does obey the ITrigger end-time however, so if 'now' is after the end-time the ITrigger will not fire again.")]
        RescheduleNowWithExistingRepeatCount,
        [Description("Re-scheduled to 'now' (even if the associated ICalendar excludes 'now') with the repeat count set to what it would be, if it had not missed any firings. This does obey the ITrigger end-time however, so if 'now' is after the end-time the ITrigger will not fire again. NOTE: Use of this instruction causes the trigger to 'forget' the start-time and repeat-count that it was originally setup with. Instead, the repeat count on the trigger will be changed to whatever the remaining repeat count is (this is only an issue if you for some reason wanted to be able to tell what the original values were at some later time). NOTE: This instruction could cause the ITrigger to go to the 'COMPLETE' state after firing 'now', if all the repeat-fire-times where missed.")]
        RescheduleNowWithRemainingRepeatCount,
        [Description("Instruct to have it's next-fire-time updated to the next time in the schedule after the current time (taking into account any associated ICalendar), but it does not want to be fired now.")]
        DoNothing,
        [Description("Instruct to fire now")]
        FireOnceNow
    }

    public enum IntervalUnit
    {
        Millisecond,
        Second,
        Minute,
        Hour,
        Day,
        Week,
        Month,
        Year
    }

    public enum DataMapType
    {
        Bool,
        String,
        Integer,
        Float,
        Double,
        Decimal,
        Long,
        Short,
        Char,
        Object
    }
}



