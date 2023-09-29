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

    private static async Task ClearTablesAsync(this DbContext dbContext, params Type[] modelTypes)
    {
        foreach (Type modelType in modelTypes)
        {
            IEntityType? entityType = dbContext.Model.FindEntityType(modelType);

            if (entityType == null)
            {
                throw new InvalidOperationException($"Table for '{modelType.Name}' not found.");
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
#pragma warning disable EF1002 // Risk of vulnerability to SQL injection.
                // Justification: Table names cannot be parameterized.
                await dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM \"{tableName}\"");
#pragma warning restore EF1002 // Risk of vulnerability to SQL injection.
            }
        }
    }
}
