using System;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using BlazingQuartz.Core;
using BlazingQuartz.Services;
using BlazingQuartz.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BlazingQuartz
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddBlazingQuartzUI(this IServiceCollection services,
			IConfiguration? blazingUIConfiguration = null,
			Action<DbContextOptionsBuilder>? dbContextOptions = null,
			string? connectionString = null)
        {
			services.Configure<BlazingQuartzUIOptions>(blazingUIConfiguration);

			var uiOptions = blazingUIConfiguration.Get<BlazingQuartzUIOptions>();
			services.AddBlazingQuartz(blazingUIConfiguration, dbContextOptions, connectionString);

			return AddBlazingQuartzUI(services);
		}

		public static IServiceCollection AddBlazingQuartzUI(this IServiceCollection services,
			Action<BlazingQuartzUIOptions>? configure = null,
			Action<DbContextOptionsBuilder>? dbContextOptions = null,
			string? connectionString = null)
		{
			if (configure == null)
			{
				services.AddOptions<BlazingQuartzUIOptions>()
					.Configure(opt =>
					{
					});
				services.AddBlazingQuartz(dbContextOptions: dbContextOptions,
					connectionString: connectionString);
			}
			else
			{
				BlazingQuartzUIOptions uiOptions = new();
				services.Configure(configure);
				services.AddBlazingQuartz(
					o =>
					{
						o.AllowedJobAssemblyFiles = uiOptions.AllowedJobAssemblyFiles;
						o.AutoMigrateDb = uiOptions.AutoMigrateDb;
						o.DataStoreProvider = uiOptions.DataStoreProvider;
						o.DisallowedJobTypes = uiOptions.DisallowedJobTypes;
					},
					dbContextOptions,
					connectionString);
			}

			return AddBlazingQuartzUI(services);
		}

		private static IServiceCollection AddBlazingQuartzUI(IServiceCollection services)
        {
			// MudBlazor
			services.AddMudServices();

			services.AddScoped<LayoutService>();
			services.AddTransient<ITriggerDetailModelValidator, TriggerDetailModelValidator>();

			return services;
		}

	}
}

