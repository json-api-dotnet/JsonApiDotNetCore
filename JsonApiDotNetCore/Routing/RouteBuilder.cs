namespace JsonApiDotNetCore.Routing
{
  public static class RouteBuilder
  {
    public static string BuildRoute(string protocol, string host, string nameSpace, string modelRouteName, string resourceId)
    {
      var id = resourceId != null ? $"/{resourceId}" : string.Empty;
      return $"{protocol}://{host}/{nameSpace}/{modelRouteName}{id}";
    }
    public static string BuildRoute(string nameSpace, string modelRouteName)
    {
      return $"/{nameSpace}/{modelRouteName}";
    }
  }
}
