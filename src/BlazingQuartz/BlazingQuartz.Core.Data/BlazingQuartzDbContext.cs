using System;
using System.Text.Json;
using BlazingQuartz.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BlazingQuartz.Core.Data
{
    public class BlazingQuartzDbContext : DbContext
    {
        public DbSet<ExecutionLog> ExecutionLogs { get; set; } = null!;

        public BlazingQuartzDbContext(DbContextOptions<BlazingQuartzDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            AddSqliteDateTimeOffsetSupport(modelBuilder);

            modelBuilder.Entity<ExecutionLog>()
                .OwnsOne(l => l.ExecutionLogDetail, e => {
                    e.ToTable("bqz_execution_log_details");
                    e.WithOwner().HasForeignKey("LogId");
                });

            modelBuilder.Entity<ExecutionLog>()
                .HasIndex(l => l.RunInstanceId)
                .IsUnique();

            // for housekeeping or system log display
            modelBuilder.Entity<ExecutionLog>()
                .HasIndex(l => new { l.DateAddedUtc, l.LogType });

            // joining with job
            modelBuilder.Entity<ExecutionLog>()
                .HasIndex(l => new { l.TriggerName, l.TriggerGroup, l.JobName, l.JobGroup, l.DateAddedUtc });

            modelBuilder
                .Entity<ExecutionLog>()
                .Property(e => e.LogType)
                .HasConversion<string>();
        }

        private void AddTablePrefix(ModelBuilder modelBuilder, string prefix)
        {
            //  Load all from the current assembly IEntityTypeConfiguration
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);

            //  Add table prefix 
            foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(prefix + entity.GetTableName());
            }
        }

        private void AddSqliteDateTimeOffsetSupport(ModelBuilder builder)
        {
            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                // SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
                // here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
                // To work around this, when the Sqlite database provider is used, all model properties of type DateTimeOffset
                // use the DateTimeOffsetToBinaryConverter
                // Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
                // This only supports millisecond precision, but should be sufficient for most use cases.
                foreach (var entityType in builder.Model.GetEntityTypes())
                {
                    var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(DateTimeOffset)
                    || p.PropertyType == typeof(DateTimeOffset?));
                    foreach (var property in properties)
                    {
                        builder
                            .Entity(entityType.Name)
                            .Property(property.Name)
                            .HasConversion(new DateTimeOffsetToBinaryConverter());
                    }
                }
            }
        }
    }
}

