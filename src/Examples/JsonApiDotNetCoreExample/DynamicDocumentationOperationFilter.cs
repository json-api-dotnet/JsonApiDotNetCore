using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCoreExample.DocAnnotations;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCoreExample;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class DynamicDocumentationOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.GetCustomAttribute<RequiresAdminAttribute>() != null)
        {
            UpdateDescription(operation, "**CAUTION**: This endpoint requires admin permissions.");
        }

        var expiresAtAttribute = context.MethodInfo.GetCustomAttribute<ExpiresOnAttribute>();

        if (expiresAtAttribute != null)
        {
            UpdateDescription(operation, $"**NOTE: This endpoint will no longer be available after {expiresAtAttribute.Value:yyyy-MM-dd}.**");
        }
    }

    private static void UpdateDescription(OpenApiOperation operation, string text)
    {
        if (string.IsNullOrEmpty(operation.Description))
        {
            operation.Description = text;
        }
        else
        {
            operation.Description += $"\n\n{text}";
        }
    }
}
