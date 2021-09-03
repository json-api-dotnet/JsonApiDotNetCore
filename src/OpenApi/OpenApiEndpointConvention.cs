using System.Linq;
using JsonApiDotNetCore;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;

namespace OpenApi
{
    internal sealed class OpenApiEndpointConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            ArgumentGuard.NotNull(action, nameof(action));

            if (!action.ActionMethod.GetCustomAttributes(true).OfType<HttpMethodAttribute>().Any())
            {
                action.ApiExplorer.IsVisible = false;
            }
        }
    }
}
