using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JsonApiDotNetCore.Abstractions;
using System.Reflection;
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
        return GetEntityById(id);
      }
      return GetRelated(id, relationalRoute.RelationshipName);
    }

    private object GetRelated(string id, string relationshipName)
    {
      // HACK: this would rely on lazy loading to work...will probably fail
      var entity = GetEntityById(id);
      var entityType = entity.GetType();
      return entityType.GetProperties().FirstOrDefault(pi => pi.Name == relationshipName).GetValue(entity);
    }

    private object GetDbSetFromContext(string propName)
    {
      var dbContext = _context.DbContext;
      return dbContext.GetType().GetProperties().FirstOrDefault(pI => pI.Name == propName)?.GetValue(dbContext, null);
    }

    private object GetEntityById(string id)
    {
      // HACK: I _believe_ by casting to IEnumerable, we are loading all records into memory, if so... find a better way...
      //        Also, we are making a BIG assumption that the resource has an attribute Id and not ResourceId which is allowed by EF
      return
        (GetDbSetFromContext(_context.Route.BaseRouteDefinition.ContextPropertyName) as IEnumerable<dynamic>)?
          .FirstOrDefault(x => x.Id.ToString() == id);
    }
  }
}
