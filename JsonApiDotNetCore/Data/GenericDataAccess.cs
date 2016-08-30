using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using JsonApiDotNetCore.Extensions;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Data
{
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

    public T SingleOrDefault<T>(object query, string param, string value)
    {
      var queryable = (IQueryable<T>) query;
      var currentType = queryable.ElementType;
      var property = currentType.GetProperty(param);

      if (property == null)
      {
        throw new ArgumentException($"'{param}' is not a valid property of '{currentType}'");
      }

      var prm = Expression.Parameter(currentType, property.Name);
      var left = Expression.Convert(Expression.PropertyOrField(prm, property.Name), typeof(string));
      var right = Expression.Constant(1, property.PropertyType);
      var body = Expression.Equal(left, right);
      var where = Expression.Lambda<Func<T, bool>>(body, prm);

      return queryable.SingleOrDefault(where);
    }
  }
}
