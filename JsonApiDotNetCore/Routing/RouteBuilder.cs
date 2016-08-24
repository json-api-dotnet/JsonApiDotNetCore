namespace JsonApiDotNetCore.Routing
{
  public static class RouteBuilder
  {
    public static string BuildRoute(string hostname, string nameSpace, string modelRouteName)
    {
      return $"{hostname}/{nameSpace}/{modelRouteName}";
    }
    public static string BuildRoute(string nameSpace, string modelRouteName)
    {
      return $"/{nameSpace}/{modelRouteName}";
    }
  }
}
