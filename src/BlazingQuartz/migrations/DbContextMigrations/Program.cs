using System;
using BlazingQuartz.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WorkerService1;

internal class Program
{
    private static void Main(string[] args)
        => CreateHostBuilder(args).Build().Run();

    #region snippet_CreateHostBuilder
    public static IHostBuilder CreateHostBuilder(string[] args)
        => Host.CreateDefaultBuilder(args)
            .ConfigureServices(
                (hostContext, services) =>
                {
                    // Set the active provider via configuration
                    var configuration = hostContext.Configuration;
                    var provider = configuration.GetValue("DataStoreProvider", "SqlServer");

                    services.AddDbContext<BlazingQuartzDbContext>(
                        options => _ = provider switch
                        {
                            "Sqlite" => options.UseSqlite(
                                "DataSource=dummy",
                                x => x.MigrationsAssembly("SqliteMigrations"))
                                .UseSnakeCaseNamingConvention(),

                            "SqlServer" => options.UseSqlServer(
                                "Data Source=dummy",
                                x => x.MigrationsAssembly("SqlServerMigrations"))
                                .UseSnakeCaseNamingConvention(),

                            "PostgreSQL" => options.UseNpgsql("Host=dummy",
                                x => x.MigrationsAssembly("PostgreSQLMigrations"))
                                .UseSnakeCaseNamingConvention(),

                            _ => throw new Exception($"Unsupported provider: {provider}")
                        });
                });
    #endregion
}