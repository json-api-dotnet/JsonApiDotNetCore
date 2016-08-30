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

    public object SingleOrDefault(string id)
    {
      var dbSet = GetDbSet();
      if (!string.IsNullOrEmpty(_includedRelationship))
      {
        dbSet = IncludeRelationshipInContext(dbSet);
      }
      var dataAccessorSingleOrDefaultMethod = _dataAccessorInstance.GetType().GetMethod("SingleOrDefault");
      var genericSingleOrDefaultMethod = dataAccessorSingleOrDefaultMethod.MakeGenericMethod(_modelType);
      return genericSingleOrDefaultMethod.Invoke(_dataAccessorInstance, new[] { dbSet, "Id", id });
    }

    private object GetDbSet()
    {
      var dataAccessorGetDbSetMethod = _dataAccessorInstance.GetType().GetMethod("GetDbSet");
      var genericGetDbSetMethod = dataAccessorGetDbSetMethod.MakeGenericMethod(_modelType);
      return genericGetDbSetMethod.Invoke(_dataAccessorInstance, new [] { _dbContext });
    }

    private object IncludeRelationshipInContext(object dbSet)
    {
      var includeMethod =  _dataAccessorInstance.GetType().GetMethod("IncludeEntity");
      var genericIncludeMethod = includeMethod.MakeGenericMethod(_modelType);
      return genericIncludeMethod.Invoke(_dataAccessorInstance, new []{ dbSet, _includedRelationship });
    }

  }
}
