using System;
using JsonApiDotNetCore.Abstractions;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Controllers
{
  public class ControllerMethodIdentifier
  {
    public ControllerMethodIdentifier(Type model, string requestMethod, PathString remainingPath, Route route)
    {
      Model = model;
      RequestMethod = requestMethod;
      RemainingPath = remainingPath;
      Route = route;
    }
    public Type Model { get; set; }
    public string RequestMethod { get; set; }
    public PathString RemainingPath { get; set; }
    public Route Route { get; set; }

    public string GetResourceId()
    {
      if(RemainingPath.HasValue)
      {
        return RemainingPath.ToUriComponent().Trim('/');
      }
      return string.Empty;
    }
  }
}
