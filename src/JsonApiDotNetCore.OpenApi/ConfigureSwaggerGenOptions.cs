using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.SwaggerComponents;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi;

internal sealed class ConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IControllerResourceMapping _controllerResourceMapping;
    private readonly JsonApiOperationIdSelector _operationIdSelector;
    private readonly JsonApiSchemaIdSelector _schemaIdSelector;

    public ConfigureSwaggerGenOptions(IControllerResourceMapping controllerResourceMapping, JsonApiOperationIdSelector operationIdSelector,
        JsonApiSchemaIdSelector schemaIdSelector)
    {
        ArgumentGuard.NotNull(controllerResourceMapping);
        ArgumentGuard.NotNull(operationIdSelector);
        ArgumentGuard.NotNull(schemaIdSelector);

        _controllerResourceMapping = controllerResourceMapping;
        _operationIdSelector = operationIdSelector;
        _schemaIdSelector = schemaIdSelector;
    }

    public void Configure(SwaggerGenOptions options)
    {
        options.SupportNonNullableReferenceTypes();
        options.UseAllOfToExtendReferenceSchemas();

        SetOperationInfo(options, _controllerResourceMapping);
        SetSchemaIdSelector(options);

        options.DocumentFilter<ServerDocumentFilter>();
        options.DocumentFilter<EndpointOrderingFilter>();
        options.OperationFilter<JsonApiOperationDocumentationFilter>();
    }

    private void SetOperationInfo(SwaggerGenOptions swaggerGenOptions, IControllerResourceMapping controllerResourceMapping)
    {
        swaggerGenOptions.TagActionsBy(description => GetOperationTags(description, controllerResourceMapping));
        swaggerGenOptions.CustomOperationIds(_operationIdSelector.GetOperationId);
    }

    private static IList<string> GetOperationTags(ApiDescription description, IControllerResourceMapping controllerResourceMapping)
    {
        MethodInfo actionMethod = description.ActionDescriptor.GetActionMethod();
        ResourceType? resourceType = controllerResourceMapping.GetResourceTypeForController(actionMethod.ReflectedType);

        if (resourceType == null)
        {
            throw new NotSupportedException("Only JsonApiDotNetCore endpoints are supported.");
        }

        return [resourceType.PublicName];
    }

    private void SetSchemaIdSelector(SwaggerGenOptions swaggerGenOptions)
    {
        swaggerGenOptions.CustomSchemaIds(_schemaIdSelector.GetSchemaId);
    }
}
