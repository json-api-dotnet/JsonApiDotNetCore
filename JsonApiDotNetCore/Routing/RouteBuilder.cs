namespace JsonApiDotNetCore.Routing
{
  public static class RouteBuilder
  {
    public static string BuildRoute(string protocol, string host, string nameSpace, string modelRouteName)
    {
      return $"{protocol}://{host}/{nameSpace}/{modelRouteName}";
    }
    public static string BuildRoute(string nameSpace, string modelRouteName)
    {
      return $"/{nameSpace}/{modelRouteName}";
    }
  }
}
