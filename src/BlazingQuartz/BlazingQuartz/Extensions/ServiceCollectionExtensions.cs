using System;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using BlazingQuartz.Core;
using BlazingQuartz.Services;
using BlazingQuartz.Models;

namespace BlazingQuartz
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddBlazingQuartzUI(this IServiceCollection services,
			Action<BlazingQuartzUIOptions>? configure = null)
		{
			BlazingQuartzUIOptions coreConfig = new();
			if (configure == null)
			{
				services.AddOptions<BlazingQuartzUIOptions>()
					.Configure(opt =>
					{
					});
			}
			else
			{
				services.Configure(configure);
				configure?.Invoke(coreConfig);
			}

			// MudBlazor
			services.AddMudServices();

			services.AddScoped<LayoutService>();
			services.AddTransient<ITriggerDetailModelValidator, TriggerDetailModelValidator>();
			services.AddBlazingQuartz(coreConfig);

            return services;
		}
	}
}

