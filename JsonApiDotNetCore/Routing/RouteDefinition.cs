using System;

namespace JsonApiDotNetCore.Routing
{
  public class RouteDefinition
  {
    public Type ModelType { get; set; }
    public string PathString { get; set; }
    public string ContextPropertyName { get; set; }
  }
}
