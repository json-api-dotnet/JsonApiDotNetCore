using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Abstractions;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCoreExample.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace JsonApiDotNetCoreExample.Controllers
{
  public class TodoItemsController : JsonApiController, IJsonApiController
  {
    private readonly ApplicationDbContext _dbContext;

    public TodoItemsController(JsonApiContext jsonApiContext, ResourceRepository resourceRepository, ApplicationDbContext applicationDbContext) : base(jsonApiContext, resourceRepository)
    {
      _dbContext = applicationDbContext;
    }

    public override ObjectResult Get()
    {
      return new OkObjectResult(_dbContext.TodoItems.ToList());
    }
  }
}
