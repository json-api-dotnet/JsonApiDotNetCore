using System;

namespace JsonApiDotNetCore.Routing
{
  public class Route
  {
    public Route(Type baseModelType, string requestMethod, string resourceId, RouteDefinition baseRouteDefinition)
    {
      BaseModelType = baseModelType;
      RequestMethod = requestMethod;
      ResourceId = resourceId;
      BaseRouteDefinition = baseRouteDefinition;
    }

    public Type BaseModelType { get; set; }
    public string RequestMethod { get; set; }
    public RouteDefinition BaseRouteDefinition { get; set; }
    public string ResourceId { get; set; }
  }
}
