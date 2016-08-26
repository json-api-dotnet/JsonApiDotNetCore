using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.JsonApi;
using Newtonsoft.Json.Linq;

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

    public static object SetValuesOnModelInstance(object model, Dictionary<string, object> jsonApiAttributes, Dictionary<string, object> jsonApiRelationships)
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

      foreach (var relationship in jsonApiRelationships)
      {
        var relationshipName = relationship.Key;
        var relationshipId = ((JObject) relationship.Value)["data"]["id"];
        var relationshipPropertyName = $"{relationshipName}Id";

        modelProperties.ForEach(pI =>
        {
          if (pI.Name.ToProperCase() == relationshipPropertyName.ToProperCase())
          {
            var convertedValue = Convert.ChangeType(relationshipId, pI.PropertyType);
            pI.SetValue(model, convertedValue);
          }
        });
      }

      return model;
    }
  }
}
