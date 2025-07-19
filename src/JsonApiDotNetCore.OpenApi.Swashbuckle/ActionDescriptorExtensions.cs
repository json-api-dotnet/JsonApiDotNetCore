using System.Reflection;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal static class ActionDescriptorExtensions
{
    public static MethodInfo GetActionMethod(this ActionDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        return ((ControllerActionDescriptor)descriptor).MethodInfo;
    }

    public static TFilterMetaData? GetFilterMetadata<TFilterMetaData>(this ActionDescriptor descriptor)
        where TFilterMetaData : IFilterMetadata
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        return descriptor.FilterDescriptors.Select(filterDescriptor => filterDescriptor.Filter).OfType<TFilterMetaData>().FirstOrDefault();
    }

    public static ControllerParameterDescriptor? GetBodyParameterDescriptor(this ActionDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        return (ControllerParameterDescriptor?)descriptor.Parameters.FirstOrDefault(parameterDescriptor =>
            parameterDescriptor.BindingInfo?.BindingSource == BindingSource.Body);
    }
}
