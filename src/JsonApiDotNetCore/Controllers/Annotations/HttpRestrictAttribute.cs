using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using JsonApiDotNetCore.Errors;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Controllers.Annotations
{
    public abstract class HttpRestrictAttribute : ActionFilterAttribute
    {
        protected abstract string[] Methods { get; }

        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (next == null) throw new ArgumentNullException(nameof(next));

            var method = context.HttpContext.Request.Method;

            if (!CanExecuteAction(method))
            {
                throw new RequestMethodNotAllowedException(new HttpMethod(method));
            }

            await next();
        }

        private bool CanExecuteAction(string requestMethod)
        {
            return !Methods.Contains(requestMethod);
        }
    }
}
