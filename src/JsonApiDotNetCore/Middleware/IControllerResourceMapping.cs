using System;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Registry of which resource is associated with which controller.
    /// </summary>
    public interface IControllerResourceMapping
    {
        /// <summary>
        /// Get the associated resource with the controller with the provided controller name
        /// </summary>
        Type GetAssociatedResource(string controllerName);
    }
}
