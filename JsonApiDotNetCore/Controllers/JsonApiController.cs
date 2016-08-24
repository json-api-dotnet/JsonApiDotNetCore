using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Controllers
{
  public class JsonApiController
  {
    private HttpContext _httpContext;
    private readonly JsonApiContext _jsonApiContext;

    public JsonApiController(HttpContext context, JsonApiContext jsonApiContext)
    {
      _jsonApiContext = jsonApiContext;
      _httpContext = context;
    }

    public ObjectResult Get()
    {
      var entities = _jsonApiContext.Get();
      return new OkObjectResult(entities);
    }

    public ObjectResult Get(string id)
    {
      var entity = _jsonApiContext.Get(id);
      return new OkObjectResult(entity);
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
