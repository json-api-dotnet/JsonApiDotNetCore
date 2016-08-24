using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Controllers
{
  public class JsonApiController
  {
    private HttpContext HttpContext;
    private JsonApiContext JsonApiContext;

    public JsonApiController(HttpContext context, JsonApiContext jsonApiContext)
    {
      JsonApiContext = jsonApiContext;
      HttpContext = context;
    }

    public ObjectResult Get()
    {
      var entities = JsonApiContext.Get();
      return new OkObjectResult(entities);
    }

    public IActionResult Get(string id)
    {
      return new OkResult();
    }

    public IActionResult Post(object entity)
    {
      return new OkResult();
    }

    public IActionResult Put(string id, object entity)
    {
      return new OkResult();
    }

    public IActionResult Delete(string id)
    {
      return new OkResult();
    }
  }
}
