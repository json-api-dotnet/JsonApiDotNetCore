using JsonApiDotNetCore.Controllers;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.ActionMethods;

/// <summary>
/// A custom JSON:API action method on <see cref="BaseJsonApiController{TResource,TId}" />.
/// </summary>
internal sealed class CustomResourceActionMethod : ResourceActionMethod
{
    public ActionDescriptor Descriptor { get; }

    public CustomResourceActionMethod(ActionDescriptor descriptor, Type controllerType)
        : base(controllerType)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        Descriptor = descriptor;
    }
}
