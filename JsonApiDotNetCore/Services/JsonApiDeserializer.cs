using System;
using System.IO;
using JsonApiDotNetCore.Abstractions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.JsonApi;
using Newtonsoft.Json;

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
      var body = GetRequestBody(_context.HttpContext.Request.Body);
      var document = JsonConvert.DeserializeObject<JsonApiDocument>(body);
      var entity = Activator.CreateInstance(_context.GetEntityType());
      return ModelAccessor.SetValuesOnModelInstance(entity, ((JsonApiDatum) document.Data).Attributes);
    }

    private static string GetRequestBody(Stream body)
    {
      using (var reader = new StreamReader(body))
      {
        return reader.ReadToEnd();
      }
    }
  }
}
