using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Routing
{
  public interface IRouter
  {
    Task<bool> HandleJsonApiRouteAsync(HttpContext context, IServiceProvider serviceProvider);
  }
}
