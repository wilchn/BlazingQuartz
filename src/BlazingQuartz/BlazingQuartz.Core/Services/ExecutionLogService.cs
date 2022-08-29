using System;
using BlazingQuartz.Core.Data;
using BlazingQuartz.Core.Data.Entities;
using BlazingQuartz.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazingQuartz.Core.Services
{
    public class ExecutionLogService : IExecutionLogService
    {
        private readonly IDbContextFactory<BlazingQuartzDbContext> _contextFactory;
        public ExecutionLogService(IDbContextFactory<BlazingQuartzDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<PagedList<ExecutionLog>> GetLatestExecutionLog(
            string jobName, string jobGroup,
            string? triggerName, string? triggerGroup,
            PageMetadata? pageMetadata = null, long firstLogId = 0)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var q = context.ExecutionLogs.Where(l => l.JobName == jobName &&
                    l.JobGroup == jobGroup);

                if (triggerName is not null)
                {
                    q = q.Where(l => l.TriggerName == triggerName &&
                        l.TriggerGroup == triggerGroup);
                }

                if (firstLogId > 0)
                {
                    // to avoid incorrect page data
                    q = q.Where(l => l.LogId <= firstLogId);
                }

                var ordered = q.OrderByDescending(l => l.DateAddedUtc).ThenByDescending(l => l.FireTimeUtc);
                if (pageMetadata == null)
                {
                    return new PagedList<ExecutionLog>(await ordered.ToListAsync());
                }

                PageMetadata newPageMetadata = pageMetadata;
                if (pageMetadata.Page == 0)
                {
                    // if first page, get the total records
                    var totalRecords = await q.CountAsync();
                    newPageMetadata = pageMetadata with { TotalCount = totalRecords };
                }

                var result = await ordered
                    .Skip(pageMetadata.Page * pageMetadata.PageSize)
                    .Take(pageMetadata.PageSize)
                    .ToListAsync();
                return new PagedList<ExecutionLog>(result, newPageMetadata);
            }
        }

        public async Task<PagedList<ExecutionLog>> GetExecutionLogs(
            ExecutionLogFilter? filter = null,
            PageMetadata? pageMetadata = null, long firstLogId = 0)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                IQueryable<ExecutionLog> q = context.ExecutionLogs;
                if (filter != null)
                {
                    if (filter.JobName != null)
                    {
                        q = q.Where(l => l.JobName == filter.JobName);
                    }

                    if (filter.JobGroup != null)
                    {
                        q = q.Where(l => l.JobGroup == filter.JobGroup);
                    }

                    if (filter.TriggerName != null)
                    {
                        q = q.Where(l => l.TriggerName == filter.TriggerName);
                    }

                    if (filter.TriggerGroup != null)
                    {
                        q = q.Where(l => l.TriggerGroup == filter.TriggerGroup);
                    }

                    if (filter.LogTypes != null && filter.LogTypes.Any())
                    {
                        q = q.Where(l => filter.LogTypes.Contains(l.LogType));
                    }

                    if (filter.DateAddedStartUtc != null)
                    {
                        q = q.Where(l => l.DateAddedUtc >= filter.DateAddedStartUtc);
                    }

                    if (filter.DateAddedEndUtc != null)
                    {
                        q = q.Where(l => l.DateAddedUtc < filter.DateAddedEndUtc);
                    }

                    if (filter.ErrorOnly)
                    {
                        q = q.Where(l => l.IsException ?? false);
                    }

                    if (filter.MessageContains != null)
                    {
                        q = q.Where(l => EF.Functions.Like(l.Result ?? String.Empty, $"%{filter.MessageContains}%")
                            || (l.ExecutionLogDetail != null && EF.Functions.Like(
                                l.ExecutionLogDetail.ExecutionDetails ?? String.Empty, $"%{filter.MessageContains}%")));
                    }
                }

                IOrderedQueryable<ExecutionLog> ordered;
                if (filter != null && filter.IsAscending)
                {
                    ordered = q.OrderBy(l => l.DateAddedUtc).ThenBy(l => l.FireTimeUtc);
                }                    
                else
                {
                    if (firstLogId > 0)
                    {
                        // to avoid incorrect page data for descing order
                        q = q.Where(l => l.LogId <= firstLogId);
                    }
                    ordered = q.OrderByDescending(l => l.DateAddedUtc).ThenByDescending(l => l.FireTimeUtc);
                }
                    

                if (pageMetadata == null)
                {
                    return new PagedList<ExecutionLog>(await ordered.ToListAsync());
                }

                PageMetadata newPageMetadata = pageMetadata;
                if (pageMetadata.Page == 0)
                {
                    // if first page, get the total records
                    var totalRecords = await q.CountAsync();
                    newPageMetadata = pageMetadata with { TotalCount = totalRecords };
                }

                var result = await ordered
                    .Skip(pageMetadata.Page * pageMetadata.PageSize)
                    .Take(pageMetadata.PageSize)
                    .ToListAsync();
                return new PagedList<ExecutionLog>(result, newPageMetadata);
            }
        }


        public async Task<IList<string>> GetJobNames()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return await context.ExecutionLogs
                    .Where(l => l.LogType != LogType.System)
                    .Select(l => l.JobName ?? string.Empty)
                    .Distinct().OrderBy(l => l)
                    .ToListAsync();
            }
        }

        public async Task<IList<string>> GetJobGroups()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return await context.ExecutionLogs
                    .Where(l => l.LogType != LogType.System)
                    .Select(l => l.JobGroup ?? string.Empty)
                    .Distinct().OrderBy(l => l)
                    .ToListAsync();
            }

        }

        public async Task<IList<string>> GetTriggerNames()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return await context.ExecutionLogs
                    .Where(l => l.LogType != LogType.System)
                    .Select(l => l.TriggerName ?? string.Empty)
                    .Distinct().OrderBy(l => l)
                    .ToListAsync();
            }
        }

        public async Task<IList<string>> GetTriggerGroups()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return await context.ExecutionLogs
                    .Where(l => l.LogType != LogType.System)
                    .Select(l => l.TriggerGroup ?? string.Empty)
                    .Distinct().OrderBy(l => l)
                    .ToListAsync();
            }
        }

    }
}

