using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Abstractions;
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
      var route = _context.Route as RelationalRoute;
      return route != null ? GetRelated(id, route) : GetEntityById(_context.Route.BaseModelType, id, null);
    }

    private object GetRelated(string id, RelationalRoute relationalRoute)
    {
      var entity = GetEntityById(relationalRoute.BaseModelType, id, relationalRoute.RelationshipName);
      return relationalRoute.BaseModelType.GetProperties().FirstOrDefault(pi => pi.Name.ToCamelCase() == relationalRoute.RelationshipName.ToCamelCase())?.GetValue(entity);
    }

    private IQueryable GetDbSetFromContext(string propName)
    {
      var dbContext = _context.DbContext;
      return (IQueryable)dbContext.GetType().GetProperties().FirstOrDefault(pI => pI.Name.ToProperCase() == propName.ToProperCase())?.GetValue(dbContext, null);
    }

    private object GetEntityById(Type modelType, string id, string includedRelationship)
    {
      // get generic dbSet
      var dataAccessorInstance = Activator.CreateInstance(typeof(GenericDataAccess));
      var dataAccessorGetDbSetMethod = dataAccessorInstance.GetType().GetMethod("GetDbSet");
      var genericGetDbSetMethod = dataAccessorGetDbSetMethod.MakeGenericMethod(modelType);
      var dbSet = genericGetDbSetMethod.Invoke(dataAccessorInstance, new [] {((DbContext) _context.DbContext) });

      // include relationships if requested
      if (!string.IsNullOrEmpty(includedRelationship))
      {
        var includeMethod =  dataAccessorInstance.GetType().GetMethod("IncludeEntity");
        var genericIncludeMethod = includeMethod.MakeGenericMethod(modelType);
        dbSet = genericIncludeMethod.Invoke(dataAccessorInstance, new []{ dbSet, includedRelationship.ToProperCase() });
      }

      // get the SingleOrDefault value by Id
      var dataAccessorSingleOrDefaultMethod = dataAccessorInstance.GetType().GetMethod("SingleOrDefault");
      var genericSingleOrDefaultMethod = dataAccessorSingleOrDefaultMethod.MakeGenericMethod(modelType);
      var entity = genericSingleOrDefaultMethod.Invoke(dataAccessorInstance, new[] { dbSet, "Id", id });

      return entity;
    }

    public void Add(object entity)
    {
      var dbSet = GetDbSetFromContext(_context.Route.BaseRouteDefinition.ContextPropertyName);
      var dbSetAddMethod = dbSet.GetType().GetMethod("Add");
      dbSetAddMethod.Invoke(dbSet, new [] { entity });
    }

    public void Delete(string id)
    {
      var entity = Get(id);
      var dbSet = GetDbSetFromContext(_context.Route.BaseRouteDefinition.ContextPropertyName);
      var dbSetAddMethod = dbSet.GetType().GetMethod("Remove");
      dbSetAddMethod.Invoke(dbSet, new [] { entity });
    }

    public int SaveChanges()
    {
      return ((DbContext)_context.DbContext).SaveChanges();
    }

  }
}
