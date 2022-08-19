using System;
using Microsoft.EntityFrameworkCore;
using BlazingQuartz.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quartz;
using BlazingQuartz.Core.History;
using BlazingQuartz.Core.Data;
using Microsoft.Extensions.Configuration;

namespace BlazingQuartz.Core
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddBlazingQuartz(this IServiceCollection services,
			Action<BlazingQuartzCoreOptions> options,
			Action<DbContextOptionsBuilder>? dbContextOptions = null,
			string? connectionString = null)
		{
			services.Configure(options);

			BlazingQuartzCoreOptions coreOptions = new();
			options.Invoke(coreOptions);

			return AddBlazingQuartz(services, coreOptions,
				dbContextOptions, connectionString);
		}

		public static IServiceCollection AddBlazingQuartz(this IServiceCollection services,
			IConfiguration? config = null,
			Action<DbContextOptionsBuilder>? dbContextOptions = null,
			string? connectionString = null)
        {
			BlazingQuartzCoreOptions coreOptions = new();
			if (config != null)
            {
				services.Configure<BlazingQuartzCoreOptions>(config);
				coreOptions = config.Get<BlazingQuartzCoreOptions>();
			}
			else
            {
				services.AddOptions<BlazingQuartzCoreOptions>()
					.Configure(opt =>
					{
					});
			}

			return AddBlazingQuartz(services, coreOptions,
				dbContextOptions, connectionString);
		}

		private static IServiceCollection AddBlazingQuartz(IServiceCollection services,
			BlazingQuartzCoreOptions coreOptions,
			Action<DbContextOptionsBuilder>? dbContextOptions = null,
			string? connectionString = null)
		{
			services.TryAddSingleton<ISchedulerDefinitionService, SchedulerDefinitionService>();
			services.AddTransient<ISchedulerService, SchedulerService>();

			var schListenerSvc = new SchedulerListenerService();
			services.TryAddSingleton<ISchedulerListenerService>(schListenerSvc);
			services.AddSingleton<ITriggerListener>(schListenerSvc);
			services.AddSingleton<IJobListener>(schListenerSvc);
			services.AddSingleton<ISchedulerListener>(schListenerSvc);
			services.AddTransient<IExecutionLogStore, ExecutionLogStore>();

			if (dbContextOptions != null)
				services.AddDbContextFactory<BlazingQuartzDbContext>(dbContextOptions);
			else
			{
				services.AddDbContextFactory<BlazingQuartzDbContext>(
					options =>
					{
						switch (coreOptions.DataStoreProvider)
                        {
							case DataStoreProvider.Sqlite:
								options.UseSqlite(connectionString ?? "DataSource=blazingQuartzApp.db;Cache=Shared",
									x => x.MigrationsAssembly("SqliteMigrations"));
								break;
							case DataStoreProvider.InMemory:
								options.UseInMemoryDatabase(connectionString ?? "BlazingQuartzDb");
								break;
							case DataStoreProvider.PostgreSQL:
								ArgumentNullException.ThrowIfNull(connectionString);
								options.UseNpgsql(connectionString,
									x => x.MigrationsAssembly("PostgreSQLMigrations"));
								break;
							default:
								throw new NotSupportedException("Unsupported data store provider. Configure services.AddDbContextFactory() manually");
						}
					});
			}

			services.AddHostedService<SchedulerEventLoggingService>();

			return services;
		}
	}
}

