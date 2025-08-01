using System.Reflection;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal static class ActionDescriptorExtensions
{
    public static MethodInfo GetActionMethod(this ActionDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        if (descriptor is ControllerActionDescriptor controllerActionDescriptor)
        {
            return controllerActionDescriptor.MethodInfo;
        }

        MethodInfo? methodInfo = descriptor.EndpointMetadata.OfType<MethodInfo>().FirstOrDefault();
        ConsistencyGuard.ThrowIf(methodInfo == null);

        return methodInfo;
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
