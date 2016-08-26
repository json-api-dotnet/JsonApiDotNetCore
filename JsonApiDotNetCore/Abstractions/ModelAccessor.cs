using System;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Extensions;

namespace JsonApiDotNetCore.Abstractions
{
  public static class ModelAccessor
  {
    public static Type GetTypeFromModelRelationshipName(Type modelType, string relationshipName)
    {
      var properties = modelType.GetProperties().Where(propertyInfo => propertyInfo.GetMethod.IsVirtual).ToList();
      var relationshipType = properties.FirstOrDefault(
        virtualProperty => virtualProperty.Name.ToCamelCase() == relationshipName.ToCamelCase())?.PropertyType;
      if(relationshipType.GetTypeInfo().IsGenericType)
      {
        return relationshipType.GetGenericArguments().Single();
      }
      return relationshipType;
    }
  }
}
