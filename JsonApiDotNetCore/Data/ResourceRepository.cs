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
      if (_context.Route is RelationalRoute)
      {
        return GetRelated(id, _context.Route as RelationalRoute);
      }
      return GetEntityById(_context.Route.BaseModelType, id, null);
    }

    private object GetRelated(string id, RelationalRoute relationalRoute)
    {
      // HACK: this would rely on lazy loading to work...will probably fail
      var entity = GetEntityById(relationalRoute.BaseModelType, id, relationalRoute.RelationshipName);
      return relationalRoute.BaseModelType.GetProperties().FirstOrDefault(pi => pi.Name.ToCamelCase() == relationalRoute.RelationshipName.ToCamelCase()).GetValue(entity);
    }

    private IQueryable GetDbSetFromContext(string propName)
    {
      var dbContext = _context.DbContext;
      return (IQueryable)dbContext.GetType().GetProperties().FirstOrDefault(pI => pI.Name.ToProperCase() == propName.ToProperCase())?.GetValue(dbContext, null);
    }

    private object GetEntityById(Type modelType, string id, string includedRelationship)
    {
      // HACK: I _believe_ by casting to IEnumerable, we are loading all records into memory, if so... find a better way...
      //        Also, we are making a BIG assumption that the resource has an attribute Id and not ResourceId which is allowed by EF
      var dataAccessorInstance = Activator.CreateInstance(typeof(GenericDataAccess));
      var dataAccessorMethod = dataAccessorInstance.GetType().GetMethod("GetDbSet");
      var genericMethod = dataAccessorMethod.MakeGenericMethod(modelType);
      var dbSet = genericMethod.Invoke(dataAccessorInstance, new [] {((DbContext) _context.DbContext) });

      if (!string.IsNullOrEmpty(includedRelationship))
      {
        var includeMethod =  dataAccessorInstance.GetType().GetMethod("IncludeEntity");
        var genericIncludeMethod = includeMethod.MakeGenericMethod(modelType);
        dbSet = genericIncludeMethod.Invoke(dataAccessorInstance, new []{ dbSet, includedRelationship.ToProperCase() });
      }

      return (dbSet as IEnumerable<dynamic>).SingleOrDefault(x => x.Id.ToString() == id);
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
