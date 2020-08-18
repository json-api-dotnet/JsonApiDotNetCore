using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using JsonApiDotNetCore.Errors;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Controllers
{
    public abstract class HttpRestrictAttribute : ActionFilterAttribute
    {
        protected abstract string[] Methods { get; }

        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var method = context.HttpContext.Request.Method;

            if (CanExecuteAction(method) == false)
            {
                throw new RequestMethodNotAllowedException(new HttpMethod(method));
            }

            await next();
        }

        private bool CanExecuteAction(string requestMethod)
        {
            return Methods.Contains(requestMethod) == false;
        }
    }
}
