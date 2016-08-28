using JsonApiDotNetCore.Abstractions;

namespace JsonApiDotNetCore.Controllers
{
  public interface IControllerBuilder
  {
    IJsonApiController BuildController(JsonApiContext context);
  }
}
