using System;
using BlazingQuartz.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quartz;

namespace BlazingQuartz.Core
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddBlazingQuartz(this IServiceCollection services,
			Action<BlazingQuartzCoreOptions>? options = null)
		{
			if (options == null)
            {
				services.AddOptions<BlazingQuartzCoreOptions>()
					.Configure(opt =>
					{
					});
			}
			else
            {
				services.Configure(options);
			}

			services.TryAddSingleton<ISchedulerDefinitionService, SchedulerDefinitionService>();
			services.AddTransient<ISchedulerService, SchedulerService>();

            var schListenerSvc = new SchedulerListenerService();
            services.TryAddSingleton<ISchedulerListenerService>(schListenerSvc);
            services.AddSingleton<ITriggerListener>(schListenerSvc);
            services.AddSingleton<IJobListener>(schListenerSvc);
            services.AddSingleton<ISchedulerListener>(schListenerSvc);

            return services;
		}
	}
}

