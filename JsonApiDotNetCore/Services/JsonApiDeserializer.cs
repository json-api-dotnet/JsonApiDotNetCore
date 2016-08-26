using System;
using System.IO;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Abstractions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.JsonApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace JsonApiDotNetCore.Services
{
  public class JsonApiDeserializer
  {
    private readonly JsonApiContext _context;
    public JsonApiDeserializer(JsonApiContext jsonApiContext)
    {
      _context = jsonApiContext;
    }

    public object GetEntityFromRequest()
    {
      var entity = Activator.CreateInstance(_context.GetEntityType());
      var datum = GetSignularJsonApiDatum();
      var attributes = datum.Attributes;
      var relationships = datum.Relationships;
      return ModelAccessor.SetValuesOnModelInstance(entity, attributes, relationships);
    }

    private JsonApiDatum GetSignularJsonApiDatum()
    {
      var document = GetJsonApiDocument();
      return ((JsonApiDatum) ((JObject) document.Data).ToObject(typeof(JsonApiDatum)));
    }

    private JsonApiDocument GetJsonApiDocument()
    {
      var body = GetRequestBody();
      return JsonConvert.DeserializeObject<JsonApiDocument>(body);
    }

    private string GetRequestBody()
    {
      using (var reader = new StreamReader(_context.HttpContext.Request.Body))
      {
        return reader.ReadToEnd();
      }
    }

    public Dictionary<PropertyInfo, object> GetEntityPatch()
    {
      var datum = GetSignularJsonApiDatum();
      var attributes = datum.Attributes;
      // var relationships = datum.Relationships;

      var patchDefinitions = new Dictionary<PropertyInfo, object>();

      var modelProperties = _context.GetEntityType().GetProperties().ToList();

      foreach (var attribute in attributes)
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

      return patchDefinitions;
    }

  }
}
