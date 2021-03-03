using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Service for specifying which routing convention to use. This can be overridden to customize the relation between controllers and mapped routes.
    /// </summary>
    [PublicAPI]
    public interface IJsonApiRoutingConvention : IApplicationModelConvention, IControllerResourceMapping
    {
    }
}
