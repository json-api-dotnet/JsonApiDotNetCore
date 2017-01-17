using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Controllers
{
    public class JsonApiControllerMixin : Controller
    {
        public JsonApiControllerMixin()
        { }

        protected IActionResult UnprocessableEntity()
        {
            return new StatusCodeResult(422);
        }
    }
}
