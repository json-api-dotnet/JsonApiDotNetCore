using System.Linq;
using JsonApiDotNetCore.Extensions;
using Microsoft.EntityFrameworkCore;

public class GenericDataAccess
{
  public DbSet<T> GetDbSet<T>(DbContext context) where T : class
  {
    return context.Set<T>();
  }

  public IQueryable<T> IncludeEntity<T>(IQueryable<T> queryable, string includedEntityName) where T : class
  {
    return queryable.Include(includedEntityName);
  }
}
