using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Controllers
{
    public abstract class JsonApiControllerMixin : Controller
    {
        protected JsonApiControllerMixin()
        { }

        protected IActionResult UnprocessableEntity()
        {
            return new StatusCodeResult(422);
        }

        protected IActionResult Forbidden()
        {
            return new StatusCodeResult(403);
        }
    }
}
