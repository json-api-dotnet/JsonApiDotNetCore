using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.JsonApi;

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

    public static object SetValuesOnModelInstance(object model, Dictionary<string, object> jsonApiAttributes)
    {
      var modelProperties = model.GetType().GetProperties().ToList();
      foreach (var attribute in jsonApiAttributes)
      {
        modelProperties.ForEach(pI =>
        {
          if (pI.Name.ToProperCase() == attribute.Key.ToProperCase())
          {
            var convertedValue = Convert.ChangeType(attribute.Value, pI.PropertyType);
            pI.SetValue(model, convertedValue);
          }
        });
      }
      return model;
    }
  }
}
