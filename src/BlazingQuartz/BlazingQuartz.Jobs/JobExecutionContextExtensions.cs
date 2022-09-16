using System;
using System.Globalization;
using Quartz;

namespace BlazingQuartz.Jobs
{
    public static class JobExecutionContextExtensions
    {
        const string ExecutionDetailsKey = "__execDetails";
        const string IsSuccessKey = "__isSuccess";
        const string ReturnCodeKey = "__returnCode";

        public static IJobExecutionContext SetReturnCode(this IJobExecutionContext context, string value)
        {
            context.Put(ReturnCodeKey, value);
            return context;
        }

        public static IJobExecutionContext SetReturnCode(this IJobExecutionContext context, int value)
        {
            context.Put(ReturnCodeKey, value.ToString());
            return context;
        }

        public static IJobExecutionContext SetExecutionDetails(this IJobExecutionContext context, string execDetails)
        {
            context.Put(ExecutionDetailsKey, execDetails);
            return context;
        }

        public static IJobExecutionContext SetIsSuccess(this IJobExecutionContext context, bool success)
        {
            context.Put(IsSuccessKey, success);
            return context;
        }

        public static string? GetReturnCode(this IJobExecutionContext context)
        {
            var val = context.Get(ReturnCodeKey);
            if (val != null)
                return Convert.ToString(val, CultureInfo.InvariantCulture);
            return null;
        }

        public static string? GetExecutionDetails(this IJobExecutionContext context)
        {
            var val = context.Get(ExecutionDetailsKey);
            if (val != null)
                return Convert.ToString(val, CultureInfo.InvariantCulture);

            return null;
        }

        public static bool? GetIsSuccess(this IJobExecutionContext context)
        {
            var value = context.Get(IsSuccessKey);
            if (value == null)
                return null;
            return Convert.ToBoolean(value);
        }
    }
}

