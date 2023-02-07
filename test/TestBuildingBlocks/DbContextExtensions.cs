using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TestBuildingBlocks;

public static class DbContextExtensions
{
    public static void AddInRange(this DbContext dbContext, params object[] entities)
    {
        dbContext.AddRange(entities);
    }

    public static async Task ClearTableAsync<TEntity>(this DbContext dbContext)
        where TEntity : class
    {
        await ClearTablesAsync(dbContext, typeof(TEntity));
    }

    public static async Task ClearTablesAsync<TEntity1, TEntity2>(this DbContext dbContext)
        where TEntity1 : class
        where TEntity2 : class
    {
        await ClearTablesAsync(dbContext, typeof(TEntity1), typeof(TEntity2));
    }

    private static async Task ClearTablesAsync(this DbContext dbContext, params Type[] models)
    {
        foreach (Type model in models)
        {
            IEntityType? entityType = dbContext.Model.FindEntityType(model);

            if (entityType == null)
            {
                throw new InvalidOperationException($"Table for '{model.Name}' not found.");
            }

            string? tableName = entityType.GetTableName();

            if (tableName == null)
            {
                // There is no table for the specified abstract base type when using TablePerConcreteType inheritance.
                IEnumerable<IEntityType> derivedTypes = entityType.GetConcreteDerivedTypesInclusive();
                await ClearTablesAsync(dbContext, derivedTypes.Select(derivedType => derivedType.ClrType).ToArray());
            }
            else
            {
                await dbContext.Database.ExecuteSqlRawAsync($"delete from \"{tableName}\"");
            }
        }
    }
}
