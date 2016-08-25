// SOURCE: https://github.com/aspnet/EntityFramework/issues/3921

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Extensions
{
  public static class EfIncludeExtension
  {
    private static MethodInfo _include = typeof(EntityFrameworkQueryableExtensions)
                        .GetMethod("Include");

    private static MethodInfo _thenIncludeReference = typeof(EntityFrameworkQueryableExtensions)
        .GetMethods()
        .Where(m => m.Name == "ThenInclude")
        .Single(m => m.Name == "ThenInclude" &&
                     m.GetParameters()
                      .Single(p => p.Name == "source")
                      .ParameterType
                      .GetGenericArguments()[1].Name != typeof(ICollection<>).Name);

    private static MethodInfo _thenIncludeCollection = typeof(EntityFrameworkQueryableExtensions)
        .GetMethods()
        .Where(m => m.Name == "ThenInclude")
        .Single(m => m.Name == "ThenInclude" &&
                     m.GetParameters()
                      .Single(p => p.Name == "source")
                      .ParameterType
                      .GetGenericArguments()[1].Name == typeof(ICollection<>).Name);

    public static IQueryable<T> Include<T>(this IQueryable<T> query, string include)
    {
      return query.Include(include.Split('.'));
    }

    public static IQueryable<T> Include<T>(this IQueryable<T> query, params string[] include)
    {
      var currentType = query.ElementType;
      var previousNavWasCollection = false;

      for (int i = 0; i < include.Length; i++)
      {
        var navigationName = include[i];
        var navigationProperty = currentType.GetProperty(navigationName);
        if (navigationProperty == null)
        {
          throw new ArgumentException($"'{navigationName}' is not a valid property of '{currentType}'");
        }

        var includeMethod = i == 0
            ? _include.MakeGenericMethod(query.ElementType, navigationProperty.PropertyType)
            : previousNavWasCollection
                ? _thenIncludeCollection.MakeGenericMethod(query.ElementType, currentType, navigationProperty.PropertyType)
                : _thenIncludeReference.MakeGenericMethod(query.ElementType, currentType, navigationProperty.PropertyType);

        var expressionParameter = Expression.Parameter(currentType);
        var expression = Expression.Lambda(
            Expression.Property(expressionParameter, navigationName),
            expressionParameter);

        query = (IQueryable<T>)includeMethod.Invoke(null, new object[] { query, expression });

        if (navigationProperty.PropertyType.GetInterfaces().Any(x => x.Name == typeof(ICollection<>).Name))
        {
          previousNavWasCollection = true;
          currentType = navigationProperty.PropertyType.GetGenericArguments().Single();
        }
        else
        {
          previousNavWasCollection = false;
          currentType = navigationProperty.PropertyType;
        }
      }

      return query;
    }
  }
}
