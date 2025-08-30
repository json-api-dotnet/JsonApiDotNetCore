using System.Reflection;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal static class ActionDescriptorExtensions
{
    public static MethodInfo? TryGetActionMethod(this ActionDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        if (descriptor is ControllerActionDescriptor controllerActionDescriptor)
        {
            return controllerActionDescriptor.MethodInfo;
        }

        return descriptor.EndpointMetadata.OfType<MethodInfo>().FirstOrDefault();
    }

    public static ControllerParameterDescriptor? GetBodyParameterDescriptor(this ActionDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        ParameterDescriptor? parameterDescriptor = descriptor.Parameters.FirstOrDefault(parameterDescriptor =>
            parameterDescriptor.BindingInfo?.BindingSource == BindingSource.Body);

        if (parameterDescriptor != null)
        {
            var controllerParameterDescriptor = parameterDescriptor as ControllerParameterDescriptor;
            ConsistencyGuard.ThrowIf(controllerParameterDescriptor == null);

            return controllerParameterDescriptor;
        }

        return null;
    }
}
