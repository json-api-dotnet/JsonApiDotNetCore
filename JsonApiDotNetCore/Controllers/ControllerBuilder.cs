using JsonApiDotNetCore.Abstractions;
using JsonApiDotNetCore.Data;

namespace JsonApiDotNetCore.Controllers
{
  public class ControllerBuilder : IControllerBuilder
  {
    public IJsonApiController BuildController(JsonApiContext context)
    {
      return new JsonApiController(context, new ResourceRepository(context));
    }
  }
}
