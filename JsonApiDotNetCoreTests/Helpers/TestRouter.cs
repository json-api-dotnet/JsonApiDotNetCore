using System;
using JsonApiDotNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCoreTests.Helpers
{
  public class TestRouter : IRouter
  {
    public bool DidHandleRoute { get; set; }
    public bool HandleJsonApiRoute(HttpContext context, IServiceProvider serviceProvider)
    {
      DidHandleRoute = true;
      return true;
    }
  }
}
