using System;
using JsonApiDotNetCore.Abstractions;
using JsonApiDotNetCore.Data;

namespace JsonApiDotNetCore.Controllers
{
  public class ControllerBuilder
  {
    private readonly JsonApiContext _context;

    public ControllerBuilder(JsonApiContext context)
    {
      _context = context;
    }

    public JsonApiController BuildController()
    {
      return new JsonApiController(_context, new ResourceRepository(_context));
    }
  }
}
