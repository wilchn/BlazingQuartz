using System;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using BlazingQuartz.Core;
using BlazingQuartz.Services;

namespace BlazingQuartz
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddBlazingQuartzUI(this IServiceCollection services,
			Action<BlazingQuartzUIOptions>? configure = null)
		{
			// MudBlazor
			services.AddMudServices();

			services.AddScoped<LayoutService>();
			services.AddBlazingQuartz();

            return services;
		}
	}
}

