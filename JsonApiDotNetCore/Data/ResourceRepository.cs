using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JsonApiDotNetCore.Abstractions;
using System.Reflection;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Data
{
  public class ResourceRepository
  {
    private readonly JsonApiContext _context;

    public ResourceRepository(JsonApiContext context)
    {
      _context = context;
    }

    public List<object> Get()
    {
      return (GetDbSetFromContext(_context.Route.BaseRouteDefinition.ContextPropertyName) as IEnumerable<object>)?.ToList();
    }

    public object Get(string id)
    {
      var relationalRoute = _context.Route as RelationalRoute;
      if (relationalRoute == null)
      {
        return GetEntityById(_context.Route.BaseModelType, id, null);
      }
      return GetRelated(id, relationalRoute);
    }

    private object GetRelated(string id, RelationalRoute relationalRoute)
    {
      // HACK: this would rely on lazy loading to work...will probably fail
      var entity = GetEntityById(relationalRoute.RelationalType, id, relationalRoute.RelationshipName);
      return relationalRoute.RelationalType.GetProperties().FirstOrDefault(pi => pi.Name.ToCamelCase() == relationalRoute.RelationshipName.ToCamelCase()).GetValue(entity);
    }


    private IQueryable GetDbSetFromContext(string propName)
    {
      var dbContext = _context.DbContext;
      return (IQueryable)dbContext.GetType().GetProperties().FirstOrDefault(pI => pI.Name.ToCamelCase() == propName)?.GetValue(dbContext, null);
    }

    private object GetEntityById(Type modelType, string id, string includedRelationship)
    {
      // HACK: I _believe_ by casting to IEnumerable, we are loading all records into memory, if so... find a better way...
      //        Also, we are making a BIG assumption that the resource has an attribute Id and not ResourceId which is allowed by EF
      var methodToCall = typeof(ResourceRepository).GetMethods().Single(method => method.Name.Equals("GetDbSet"));
      var genericMethod = methodToCall.MakeGenericMethod(modelType);
      genericMethod.Invoke(genericMethod, null);
      var dbSet = genericMethod.Invoke(this, null);

      if (!string.IsNullOrEmpty(includedRelationship))
      {
        var includeMethod = typeof(ResourceRepository).GetMethods().Single(method => method.Name.Equals("IncludeEntity"));
        var genericIncludeMethod = includeMethod.MakeGenericMethod(modelType);
        genericIncludeMethod.Invoke(genericMethod, null);
        dbSet = genericIncludeMethod.Invoke(this, new []{ dbSet, includedRelationship });
      }

      return (dbSet as IEnumerable<dynamic>).SingleOrDefault(x => x.Id.ToString() == id);
    }

    private DbSet<T> GetDbSet<T>() where T : class
    {
      return ((DbContext) _context.DbContext).Set<T>();
    }

    private IQueryable<T> IncludeEntity<T>(IQueryable<T> queryable, string includedEntityName) where T : class
    {
      return queryable.Include(includedEntityName);
    }
  }
}
