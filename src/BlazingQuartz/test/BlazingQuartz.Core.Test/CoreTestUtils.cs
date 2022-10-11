using System;
using BlazingQuartz.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazingQuartz.Core.Test
{
    public static class CoreTestUtils
    {
        public static BlazingQuartzDbContext GetInMemoryBlazingQuartzDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<BlazingQuartzDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new BlazingQuartzDbContext(options);
        }

    }
}

