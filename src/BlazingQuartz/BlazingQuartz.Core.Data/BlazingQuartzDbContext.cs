using System;
using System.Text.Json;
using BlazingQuartz.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

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

            //  Load all from the current assembly IEntityTypeConfiguration
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);

            //  Add table prefix 
            foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName("bqz_" + entity.GetTableName());
            }

            modelBuilder.Entity<ExecutionLog>()
                .Property(l => l.ExceptionMessage)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<ExceptionMessage>(v, (JsonSerializerOptions?)null));

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
    }
}

