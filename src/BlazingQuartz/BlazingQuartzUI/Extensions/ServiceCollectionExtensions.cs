using System;
using BlazingQuartzUI.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace BlazingQuartz
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddBlazingQuartzUI(this IServiceCollection services)
		{
			// MudBlazor
			services.AddMudServices();

			services.AddScoped<LayoutService>();
            //services.AddTransient<ISchedulerDefinitionService, SchedulerDefinitionService>();

            //var schListenerSvc = new SchedulerListenerService();
            //services.TryAddSingleton<ISchedulerListenerService>(schListenerSvc);
            //services.AddSingleton<ITriggerListener>(schListenerSvc);
            //services.AddSingleton<IJobListener>(schListenerSvc);
            //services.AddSingleton<ISchedulerListener>(schListenerSvc);

            return services;
		}
	}
}

