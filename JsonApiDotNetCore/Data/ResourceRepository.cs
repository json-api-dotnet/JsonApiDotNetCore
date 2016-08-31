using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Abstractions;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Collections;

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
      IQueryable dbSet;
      var filter = _context.Route.Query.Filter;
      if(filter != null) {
        dbSet = FilterEntities(_context.Route.BaseModelType, filter.PropertyName, filter.PropertyValue, null);
      }
      else {
        dbSet = GetDbSet(_context.Route.BaseModelType, null);
      }
      return ((IEnumerable<object>)dbSet).ToList();
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

    private IQueryable GetDbSet(Type modelType, string includedRelationship)
    {
      var dbContext = _context.DbContext;
      return (IQueryable)new GenericDataAccessAbstraction(_context.DbContext, modelType, includedRelationship).GetDbSet();
    }

    private object GetEntityById(Type modelType, string id, string includedRelationship)
    {
      return new GenericDataAccessAbstraction(_context.DbContext, modelType, includedRelationship).SingleOrDefault("Id", id);
    }

    private IQueryable FilterEntities(Type modelType, string property, string value, string includedRelationship)
    {
      return new GenericDataAccessAbstraction(_context.DbContext, modelType, includedRelationship).Filter(property, value);
    }

    public void Add(object entity)
    {
      var dbSet = GetDbSet(_context.Route.BaseModelType, null);
      var dbSetAddMethod = dbSet.GetType().GetMethod("Add");
      dbSetAddMethod.Invoke(dbSet, new [] { entity });
    }

    public void Delete(string id)
    {
      var entity = Get(id);
      var dbSet = GetDbSet(_context.Route.BaseModelType, null);
      var dbSetAddMethod = dbSet.GetType().GetMethod("Remove");
      dbSetAddMethod.Invoke(dbSet, new [] { entity });
    }

    public int SaveChanges()
    {
      return ((DbContext)_context.DbContext).SaveChanges();
    }

  }
}
