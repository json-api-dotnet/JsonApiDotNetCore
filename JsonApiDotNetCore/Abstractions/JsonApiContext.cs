using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Abstractions
{
  public class JsonApiContext
  {
    public HttpContext HttpContext { get; }
    public Route Route { get; }
    public object DbContext { get; }
    public JsonApiModelConfiguration Configuration { get; }

    public JsonApiContext(HttpContext httpContext, Route route, object dbContext, JsonApiModelConfiguration configuration)
    {
      HttpContext = httpContext;
      Route = route;
      DbContext = dbContext;
      Configuration = configuration;
    }

    public Type GetJsonApiResourceType()
    {
      return Configuration.ResourceMapDefinitions[Route.BaseModelType];
    }
  }
}
