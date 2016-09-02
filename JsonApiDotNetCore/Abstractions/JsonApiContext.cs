using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System.Linq;
using JsonApiDotNetCore.Extensions;

namespace JsonApiDotNetCore.Abstractions
{
  public class JsonApiContext : IJsonApiContext
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
      return Configuration.ResourceMapDefinitions[Route.BaseModelType].Item1;
    }

    public string GetEntityName()
    {
      return (!(Route is RelationalRoute) ? Route.BaseRouteDefinition.ContextPropertyName
      : Configuration.Routes.Single(r => r.ModelType == ((RelationalRoute)Route).RelationalType).ContextPropertyName).Dasherize();
    }

    public Type GetEntityType()
    {
      return !(Route is RelationalRoute) ? Route.BaseRouteDefinition.ModelType
      : ((RelationalRoute)Route).RelationalType;
    }
  }
}
