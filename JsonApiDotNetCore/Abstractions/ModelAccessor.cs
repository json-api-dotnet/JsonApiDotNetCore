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
        virtualProperty => virtualProperty.Name.Dasherize() == relationshipName.Dasherize())?.PropertyType;
      if(relationshipType.GetTypeInfo().IsGenericType)
      {
        return relationshipType.GetGenericArguments().Single();
      }
      return relationshipType;
    }

    public static object SetValuesOnModelInstance(object model, Dictionary<string, object> jsonApiAttributes, Dictionary<string, object> jsonApiRelationships)
    {
      var patches = GetEntityPatch(model.GetType(), jsonApiAttributes, jsonApiRelationships);
      foreach(var patch in patches)
      {
         patch.Key.SetValue(model, patch.Value);
      }

      return model;
    }

    public static Dictionary<PropertyInfo, object> GetEntityPatch(Type modelType, Dictionary<string, object> jsonApiAttributes, Dictionary<string, object> jsonApiRelationships)
    {
      var patchDefinitions = new Dictionary<PropertyInfo, object>();

      var modelProperties = modelType.GetProperties().ToList();
      foreach (var attribute in jsonApiAttributes)
      {
        modelProperties.ForEach(pI =>
        {
          if (pI.Name.ToProperCase() == attribute.Key.ToProperCase())
          {
            var convertedValue = Convert.ChangeType(attribute.Value, pI.PropertyType);
            patchDefinitions.Add(pI, convertedValue);
          }
        });
      }

      if(jsonApiRelationships != null) {
        foreach (var relationship in jsonApiRelationships)
        {
          var relationshipId = ((JObject) relationship.Value)["data"]["id"].ToString();
          var relationshipTypeName = ((JObject) relationship.Value)["data"]["type"];
          var relationshipPropertyName = $"{relationshipTypeName}Id";

          modelProperties.ForEach(pI =>
          {
            if (pI.Name.ToProperCase() == relationshipPropertyName.ToProperCase())
            {
              var convertedValue = Convert.ChangeType(relationshipId, pI.PropertyType);
              patchDefinitions.Add(pI, convertedValue);
            }
          });
        }
      }

      return patchDefinitions;
    }
  }
}
