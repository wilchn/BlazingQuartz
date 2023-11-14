using System;
using Microsoft.EntityFrameworkCore;
using BlazingQuartz.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quartz;
using BlazingQuartz.Core.History;
using BlazingQuartz.Core.Data;
using BlazingQuartz.Jobs;
using Microsoft.Extensions.Configuration;
using System.Reflection;

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
			services.AddBlazingQuartzJobs();

            services.TryAddSingleton<ISchedulerDefinitionService, SchedulerDefinitionService>();
			services.AddTransient<ISchedulerService, SchedulerService>();

			var schListenerSvc = new SchedulerListenerService();
			services.TryAddSingleton<ISchedulerListenerService>(schListenerSvc);
			services.AddSingleton<ITriggerListener>(schListenerSvc);
			services.AddSingleton<IJobListener>(schListenerSvc);
			services.AddSingleton<ISchedulerListener>(schListenerSvc);
			services.AddTransient<IExecutionLogStore, ExecutionLogStore>();
			services.AddTransient<IExecutionLogService, ExecutionLogService>();

			services.AddSingleton<IExecutionLogRawSqlProvider, BaseExecutionLogRawSqlProvider>();

			if (dbContextOptions != null)
            {
				services.AddDbContextFactory<BlazingQuartzDbContext>(dbContextOptions);
			}	
			else
			{
				Action<DbContextOptionsBuilder>? dbOptionAction = null;
				switch (coreOptions.DataStoreProvider)
				{
					case DataStoreProvider.Sqlite:
						dbOptionAction = options =>
							options.UseSqlite(connectionString ?? "DataSource=blazingQuartzApp.db;Cache=Shared",
								x => x.MigrationsAssembly("SqliteMigrations"))
								.UseSnakeCaseNamingConvention();
						break;
					case DataStoreProvider.InMemory:
						dbOptionAction = options =>
							options.UseInMemoryDatabase(connectionString ?? "BlazingQuartzDb");
						break;
					case DataStoreProvider.PostgreSQL:
						ArgumentNullException.ThrowIfNull(connectionString);
						dbOptionAction = options =>
							options.UseNpgsql(connectionString,
								x => x.MigrationsAssembly("PostgreSQLMigrations"))
								.UseSnakeCaseNamingConvention();
						break;
                    case DataStoreProvider.SqlServer:
                        ArgumentNullException.ThrowIfNull(connectionString);
                        dbOptionAction = options =>
                            options.UseSqlServer(connectionString,
                                x => x.MigrationsAssembly("SqlServerMigrations"))
                                .UseSnakeCaseNamingConvention();
                        break;
                    default:
						throw new NotSupportedException("Unsupported data store provider. Configure services.AddDbContextFactory() manually");

				}

				services.AddDbContextFactory<BlazingQuartzDbContext>(dbOptionAction);
			}

			services.AddHostedService<SchedulerEventLoggingService>();
			LoadJobAssemblies(coreOptions);

			return services;
		}

		private static void LoadJobAssemblies(BlazingQuartzCoreOptions coreOptions)
        {
			if (coreOptions.AllowedJobAssemblyFiles == null)
				return;

			var path = Path.GetDirectoryName(Assembly.GetAssembly(typeof(SchedulerDefinitionService))!.Location) ?? String.Empty;
			List<Type> jobTypes = new();
			foreach (var assemblyStr in coreOptions.AllowedJobAssemblyFiles)
			{
				string assemblyPath = Path.Combine(path, assemblyStr + ".dll");
				Assembly.LoadFrom(assemblyPath);
			}
		}
	}
}

