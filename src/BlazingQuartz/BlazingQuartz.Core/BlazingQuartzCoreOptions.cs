using System;
namespace BlazingQuartz.Core
{
    public class BlazingQuartzCoreOptions
    {
        /// <summary>
        /// <para>Assembly files that contain IJob implementation use for creating new Jobs.</para>
        /// <para>Ex. Quartz.Jobs</para>
        /// <para>Or Jobs/Quartz.Jobs - if this file is under Job folder</para>
        /// </summary>
        public string[]? AllowedJobAssemblyFiles { get; set; }
        /// <summary>
        /// <para>Job types that are not allowed to be used for creating new Jobs using UI.</para>
        /// <para>Ex. Quartz.Job.NativeJob</para>
        /// </summary>
        public string[]? DisallowedJobTypes { get; set; }
    }
}

