using System;
using JsonApiDotNetCore.Routing.Query;

namespace JsonApiDotNetCore.Routing
{
  public class Route
  {
    public Route(Type baseModelType, string requestMethod, string resourceId, RouteDefinition baseRouteDefinition, QuerySet query)
    {
      BaseModelType = baseModelType;
      RequestMethod = requestMethod;
      ResourceId = resourceId;
      BaseRouteDefinition = baseRouteDefinition;
      Query = query;
    }

    public Type BaseModelType { get; set; }
    public string RequestMethod { get; set; }
    public RouteDefinition BaseRouteDefinition { get; set; }
    public string ResourceId { get; set; }
    public QuerySet Query { get; set; }
  }
}
