using System;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Controllers
{
  public class JsonApiController : Controller
  {
    public IActionResult Get()
    {
      return Ok();
    }

    public IActionResult Get(string id)
    {
      return Ok();
    }

    public IActionResult Post(object entity)
    {
      return Ok();
    }

    public IActionResult Put(string id, object entity)
    {
      return Ok();
    }

    public IActionResult Delete(string id)
    {
      return Ok();
    }
  }
}
