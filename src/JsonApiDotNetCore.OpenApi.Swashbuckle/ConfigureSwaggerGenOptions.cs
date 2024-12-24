using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.AtomicOperations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal sealed class ConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
{
    private static readonly Type[] AtomicOperationDerivedSchemaTypes =
    [
        typeof(CreateResourceOperation<>),
        typeof(UpdateResourceOperation<>),
        typeof(DeleteResourceOperation<>),
        typeof(UpdateToOneRelationshipOperation<>),
        typeof(UpdateToManyRelationshipOperation<>),
        typeof(AddToRelationshipOperation<>),
        typeof(RemoveFromRelationshipOperation<>)
    ];

    private readonly OpenApiOperationIdSelector _operationIdSelector;
    private readonly JsonApiSchemaIdSelector _schemaIdSelector;
    private readonly IControllerResourceMapping _controllerResourceMapping;
    private readonly IResourceGraph _resourceGraph;

    public ConfigureSwaggerGenOptions(OpenApiOperationIdSelector operationIdSelector, JsonApiSchemaIdSelector schemaIdSelector,
        IControllerResourceMapping controllerResourceMapping, IResourceGraph resourceGraph)
    {
        ArgumentNullException.ThrowIfNull(operationIdSelector);
        ArgumentNullException.ThrowIfNull(schemaIdSelector);
        ArgumentNullException.ThrowIfNull(controllerResourceMapping);
        ArgumentNullException.ThrowIfNull(resourceGraph);

        _operationIdSelector = operationIdSelector;
        _schemaIdSelector = schemaIdSelector;
        _controllerResourceMapping = controllerResourceMapping;
        _resourceGraph = resourceGraph;
    }

    public void Configure(SwaggerGenOptions options)
    {
        options.SupportNonNullableReferenceTypes();
        options.UseAllOfToExtendReferenceSchemas();

        options.UseAllOfForInheritance();
        options.SelectDiscriminatorNameUsing(_ => JsonApiPropertyName.Type);
        options.SelectDiscriminatorValueUsing(clrType => _resourceGraph.GetResourceType(clrType).PublicName);
        options.SelectSubTypesUsing(SelectDerivedTypes);

        options.TagActionsBy(description => GetOpenApiOperationTags(description, _controllerResourceMapping));
        options.CustomOperationIds(_operationIdSelector.GetOpenApiOperationId);
        options.CustomSchemaIds(_schemaIdSelector.GetSchemaId);

        options.DocumentFilter<ServerDocumentFilter>();
        options.DocumentFilter<EndpointOrderingFilter>();
        options.DocumentFilter<StringEnumOrderingFilter>();
        options.OperationFilter<DocumentationOpenApiOperationFilter>();
        options.DocumentFilter<UnusedComponentSchemaCleaner>();
    }

    private List<Type> SelectDerivedTypes(Type baseType)
    {
        if (baseType == typeof(ResourceData))
        {
            return GetConstructedTypesForResourceData();
        }

        if (baseType == typeof(AtomicOperation))
        {
            return GetConstructedTypesForAtomicOperation();
        }

        return [];
    }

    private List<Type> GetConstructedTypesForResourceData()
    {
        List<Type> derivedTypes = [];

        foreach (ResourceType resourceType in _resourceGraph.GetResourceTypes())
        {
            Type constructedType = typeof(ResourceDataInResponse<>).MakeGenericType(resourceType.ClrType);
            derivedTypes.Add(constructedType);
        }

        return derivedTypes;
    }

    private List<Type> GetConstructedTypesForAtomicOperation()
    {
        List<Type> derivedTypes = [];

        foreach (ResourceType resourceType in _resourceGraph.GetResourceTypes())
        {
            derivedTypes.AddRange(AtomicOperationDerivedSchemaTypes.Select(openType => openType.MakeGenericType(resourceType.ClrType)));
        }

        return derivedTypes;
    }

    private static List<string> GetOpenApiOperationTags(ApiDescription description, IControllerResourceMapping controllerResourceMapping)
    {
        MethodInfo actionMethod = description.ActionDescriptor.GetActionMethod();
        ResourceType? resourceType = controllerResourceMapping.GetResourceTypeForController(actionMethod.ReflectedType);

        return resourceType == null ? ["operations"] : [resourceType.PublicName];
    }
}
