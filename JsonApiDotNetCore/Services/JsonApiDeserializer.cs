using System;
using JsonApiDotNetCore.Abstractions;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Services
{
  public class JsonApiDeserializer
  {
    private readonly JsonApiContext _context;
    private readonly JsonApiModelConfiguration _jsonApiModelConfiguration;
    private readonly string _entityName;
    private readonly Type _entityType;
    public JsonApiDeserializer(JsonApiContext jsonApiContext, JsonApiModelConfiguration configuration)
    {
      _context = jsonApiContext;
      _jsonApiModelConfiguration = configuration;
      _entityName = jsonApiContext.GetEntityName();
      _entityType = jsonApiContext.GetEntityType();
    }
  }
}
