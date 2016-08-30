using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Routing
{
  public interface IRouteBuilder
  {
    Route BuildFromRequest(HttpRequest request);
  }
}
