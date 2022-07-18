using System;
using BlazingQuartz.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quartz;

namespace BlazingQuartz.Core
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddBlazingQuartz(this IServiceCollection services)
		{
			services.AddTransient<ISchedulerDefinitionService, SchedulerDefinitionService>();

            var schListenerSvc = new SchedulerListenerService();
            services.TryAddSingleton<ISchedulerListenerService>(schListenerSvc);
            services.AddSingleton<ITriggerListener>(schListenerSvc);
            services.AddSingleton<IJobListener>(schListenerSvc);
            services.AddSingleton<ISchedulerListener>(schListenerSvc);

            return services;
		}
	}
}

