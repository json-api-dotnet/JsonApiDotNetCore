using System;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Routing
{
  public class Route
  {
    public Route(Type model, string requestMethod, PathString remainingPath, RouteDefinition routeDefinition)
    {
      Model = model;
      RequestMethod = requestMethod;
      RemainingPath = remainingPath;
      RouteDefinition = routeDefinition;
      SetResourceId();
    }

    public Type Model { get; set; }
    public string RequestMethod { get; set; }
    public PathString RemainingPath { get; set; }
    public RouteDefinition RouteDefinition { get; set; }
    public string ResourceId { get; set; }

    private void SetResourceId()
    {
      ResourceId = RemainingPath.HasValue ? RemainingPath.ToUriComponent().Trim('/') : null;
    }
  }
}
