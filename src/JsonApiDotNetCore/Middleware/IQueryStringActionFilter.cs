using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    public interface IQueryStringActionFilter
    {
        Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next);
    }
}