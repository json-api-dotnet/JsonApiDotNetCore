using System;
using System.Threading.Tasks;
using JsonApiDotNetCoreExample.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace JsonApiDotNetCoreExampleTests
{
    public static class AppDbContextExtensions
    {
        public static async Task ClearTableAsync<TEntity>(this AppDbContext dbContext) where TEntity : class
        {
            var entityType = dbContext.Model.FindEntityType(typeof(TEntity));
            if (entityType == null)
            {
                throw new InvalidOperationException($"Table for '{typeof(TEntity).Name}' not found.");
            }

            string tableName = entityType.GetTableName();

            // PERF: We first try to clear the table, which is fast and usually succeeds, unless foreign key constraints are violated.
            // In that case, we recursively delete all related data, which is slow.
            try
            {
                await dbContext.Database.ExecuteSqlRawAsync("delete from \"" + tableName + "\"");
            }
            catch (PostgresException)
            {
                await dbContext.Database.ExecuteSqlRawAsync("truncate table \"" + tableName + "\" cascade");
            }
        }

        public static void ClearTable<TEntity>(this AppDbContext dbContext) where TEntity : class
        {
            var entityType = dbContext.Model.FindEntityType(typeof(TEntity));
            if (entityType == null)
            {
                throw new InvalidOperationException($"Table for '{typeof(TEntity).Name}' not found.");
            }

            string tableName = entityType.GetTableName();

            // PERF: We first try to clear the table, which is fast and usually succeeds, unless foreign key constraints are violated.
            // In that case, we recursively delete all related data, which is slow.
            try
            {
                dbContext.Database.ExecuteSqlRaw("delete from \"" + tableName + "\"");
            }
            catch (PostgresException)
            {
                dbContext.Database.ExecuteSqlRaw("truncate table \"" + tableName + "\" cascade");
            }
        }
    }
}
