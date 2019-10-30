using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace JsonApiDotNetCore.Internal
{
    /// <summary>
    /// Service for specifying which routing convention to use. This can be overriden to customize
    /// the relation between controllers and mapped routes.
    /// </summary>
    public interface IJsonApiRoutingConvention : IApplicationModelConvention, IControllerResourceMapping { }
}
