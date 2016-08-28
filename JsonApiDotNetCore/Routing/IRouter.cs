using System;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Routing
{
  public interface IRouter
  {
    bool HandleJsonApiRoute(HttpContext context, IServiceProvider serviceProvider);
  }
}
