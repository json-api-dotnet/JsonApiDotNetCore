using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JsonApiDotNetCore.Attributes;

namespace JsonApiDotNetCore.Abstractions
{
  public static class ModelAccessor
  {
    public static Type GetTypeFromModelRelationshipName(object model, string relationshipName)
    {
      return model.GetType().GetProperties().Where(propertyInfo => propertyInfo.GetMethod.IsVirtual).ToList().FirstOrDefault(
        virtualProperty => virtualProperty.Name == relationshipName)?.PropertyType;
    }

  }
}
