using JsonApiDotNetCore.Abstractions;
using JsonApiDotNetCore.Data;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Controllers
{
  public class JsonApiController
  {
    protected readonly JsonApiContext JsonApiContext;
    private readonly ResourceRepository _resourceRepository;

    public JsonApiController(JsonApiContext jsonApiContext, ResourceRepository resourceRepository)
    {
      JsonApiContext = jsonApiContext;
      _resourceRepository = resourceRepository;
    }

    public ObjectResult Get()
    {
      var entities = _resourceRepository.Get();
      return new OkObjectResult(entities);
    }

    public ObjectResult Get(string id)
    {
      var entity = _resourceRepository.Get(id);
      return new OkObjectResult(entity);
    }

    public ObjectResult Post(object entity)
    {
      return new CreatedResult(JsonApiContext.HttpContext.Request.Path, entity);
    }

    public ObjectResult Put(string id, object entity)
    {
      return new OkObjectResult(entity);
    }

    public ObjectResult Delete(string id)
    {
      return new OkObjectResult(null);
    }
  }
}
