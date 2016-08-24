namespace JsonApiDotNetCore.Routing
{
  public static class RouteBuilder
  {
    public static string BuildRoute(string nameSpace, string resourceCollectionName)
    {
      return $"/{nameSpace}/{resourceCollectionName}";
    }
  }
}
