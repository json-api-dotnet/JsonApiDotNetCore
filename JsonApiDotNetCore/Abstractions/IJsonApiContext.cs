using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Abstractions
{
  public interface IJsonApiContext
  {
    JsonApiModelConfiguration Configuration { get; }
    object DbContext { get; }
    HttpContext HttpContext { get; }
    Route Route { get; }
    string GetEntityName();
    Type GetEntityType();
    Type GetJsonApiResourceType();
  }
}
