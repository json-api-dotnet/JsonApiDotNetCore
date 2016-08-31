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

    public T SingleOrDefault<T>(object query, string param, object value)
    {
      var queryable = (IQueryable<T>) query;
      var expression = GetEqualityExpressionForProperty(queryable, param, value);
      return queryable.SingleOrDefault(expression);
    }

    public IQueryable<T> Where<T>(object query, string param, object value)
    {
      var queryable = (IQueryable<T>) query;
      var expression = GetEqualityExpressionForProperty(queryable, param, value);
      return queryable.Where(expression);
    }

    private Expression<Func<T,bool>> GetEqualityExpressionForProperty<T>(IQueryable<T> query, string param, object value)
    {
      var currentType = query.ElementType;
      var property = currentType.GetProperty(param);

      if (property == null)
      {
        throw new ArgumentException($"'{param}' is not a valid property of '{currentType}'");
      }

      // convert the incoming value to the target value type
      // "1" -> 1
      var convertedValue = Convert.ChangeType(value, property.PropertyType);
      // {model}
      var prm = Expression.Parameter(currentType, "model");
      // {model.Id}
      var left = Expression.PropertyOrField(prm, property.Name);
      // {1}
      var right = Expression.Constant(convertedValue, property.PropertyType);
      // {model.Id == 1}
      var body = Expression.Equal(left, right);
      return Expression.Lambda<Func<T, bool>>(body, prm);
    }
  }
}
