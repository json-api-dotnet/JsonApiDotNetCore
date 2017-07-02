using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Controllers
{
    public abstract class HttpRestrictAttribute : ActionFilterAttribute, IAsyncActionFilter
    {
        protected abstract string[] Methods { get; }

        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var method = context.HttpContext.Request.Method;
            
            if(CanExecuteAction(method) == false)
                throw new JsonApiException(405, $"This resource does not support {method} requests.");

            await next();
        }

        private bool CanExecuteAction(string requestMethod)
        {            
            return Methods.Contains(requestMethod) == false;
        }
    }

    public class HttpReadOnlyAttribute : HttpRestrictAttribute
    {
        protected override string[] Methods { get; } = new string[] { "POST", "PATCH", "DELETE" };
    }

    public class NoHttpPostAttribute : HttpRestrictAttribute
    {
        protected override string[] Methods { get; } = new string[] { "POST" };
    }

    public class NoHttpPatchAttribute : HttpRestrictAttribute
    {
        protected override string[] Methods { get; } = new string[] { "PATCH" };
    }

    public class NoHttpDeleteAttribute : HttpRestrictAttribute
    {
        protected override string[] Methods { get; } = new string[] { "DELETE" };
    }
}
