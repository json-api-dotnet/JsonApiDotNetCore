using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using JsonApiDotNetCore.Extensions;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Data
{
  public class GenericDataAccessAbstraction
  {
    private GenericDataAccess _dataAccessorInstance;
    private DbContext _dbContext;
    private Type _modelType;
    private string _includedRelationship;
    public GenericDataAccessAbstraction(object dbContext, Type modelType, string includedRelationship)
    {
      _dataAccessorInstance = (GenericDataAccess)Activator.CreateInstance(typeof(GenericDataAccess));
      _dbContext = (DbContext) dbContext;
      _modelType = modelType;
      _includedRelationship = includedRelationship?.ToProperCase();
    }

    public object SingleOrDefault(string propertyName, string value)
    {
      var dbSet = GetDbSet();
      return InvokeGenericDataAccessMethod("SingleOrDefault", new[] { dbSet, propertyName, value });
    }

    public IQueryable Filter(string propertyName, string value)
    {
      var dbSet = GetDbSet();
      return (IQueryable)InvokeGenericDataAccessMethod("Where", new[] { dbSet, propertyName, value });
    }

    private object InvokeGenericDataAccessMethod(string methodName, params object[] propertyValues)
    {
      var dataAccessorMethod = _dataAccessorInstance.GetType().GetMethod(methodName);
      var genericDataAccessorMethod = dataAccessorMethod.MakeGenericMethod(_modelType);
      return genericDataAccessorMethod.Invoke(_dataAccessorInstance, propertyValues);
    }

    public object GetDbSet()
    {
      var dataAccessorGetDbSetMethod = _dataAccessorInstance.GetType().GetMethod("GetDbSet");
      var genericGetDbSetMethod = dataAccessorGetDbSetMethod.MakeGenericMethod(_modelType);
      var dbSet = genericGetDbSetMethod.Invoke(_dataAccessorInstance, new [] { _dbContext });
      if (!string.IsNullOrEmpty(_includedRelationship))
      {
        dbSet = IncludeRelationshipInContext(dbSet);
      }
      return dbSet;
    }

    private object IncludeRelationshipInContext(object dbSet)
    {
      var includeMethod =  _dataAccessorInstance.GetType().GetMethod("IncludeEntity");
      var genericIncludeMethod = includeMethod.MakeGenericMethod(_modelType);
      return genericIncludeMethod.Invoke(_dataAccessorInstance, new []{ dbSet, _includedRelationship });
    }

  }
}
