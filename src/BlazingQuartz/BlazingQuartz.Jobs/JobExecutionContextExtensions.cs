﻿using System;
using System.Globalization;
using Quartz;
using BlazingQuartz.Jobs.Abstractions;

namespace BlazingQuartz.Jobs
{
    public static class JobExecutionContextExtensions
    {
        public static IJobExecutionContext SetReturnCode(this IJobExecutionContext context, string value)
        {
            context.Put(JobDataMapKeys.ReturnCode, value);
            return context;
        }

        public static IJobExecutionContext SetReturnCode(this IJobExecutionContext context, int value)
        {
            context.Put(JobDataMapKeys.ReturnCode, value.ToString());
            return context;
        }

        public static IJobExecutionContext SetExecutionDetails(this IJobExecutionContext context, string execDetails)
        {
            context.Put(JobDataMapKeys.ExecutionDetails, execDetails);
            return context;
        }

        public static IJobExecutionContext SetIsSuccess(this IJobExecutionContext context, bool success)
        {
            context.Put(JobDataMapKeys.IsSuccess, success);
            return context;
        }

        public static string? GetReturnCode(this IJobExecutionContext context)
        {
            var val = context.Get(JobDataMapKeys.ReturnCode);
            if (val != null)
                return Convert.ToString(val, CultureInfo.InvariantCulture);
            return null;
        }

        public static string? GetExecutionDetails(this IJobExecutionContext context)
        {
            var val = context.Get(JobDataMapKeys.ExecutionDetails);
            if (val != null)
                return Convert.ToString(val, CultureInfo.InvariantCulture);

            return null;
        }

        public static bool? GetIsSuccess(this IJobExecutionContext context)
        {
            var value = context.Get(JobDataMapKeys.IsSuccess);
            if (value == null)
                return null;
            return Convert.ToBoolean(value);
        }
    }
}
