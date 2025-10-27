using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.ActionMethods;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.AtomicOperations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal sealed class ConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
{
    private static readonly Dictionary<Type, Type> BaseToDerivedSchemaTypes = new()
    {
        [typeof(IdentifierInRequest)] = typeof(IdentifierInRequest<>),
        [typeof(ResourceInResponse)] = typeof(DataInResponse<>)
    };

    private static readonly Type[] AtomicOperationDerivedSchemaTypes =
    [
        typeof(CreateOperation<>),
        typeof(UpdateOperation<>),
        typeof(DeleteOperation<>),
        typeof(UpdateToOneRelationshipOperation<>),
        typeof(UpdateToManyRelationshipOperation<>),
        typeof(AddToRelationshipOperation<>),
        typeof(RemoveFromRelationshipOperation<>)
    ];

    private static readonly Func<ApiDescription, IList<string>> DefaultTagsSelector = new SwaggerGeneratorOptions().TagsSelector;

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
        ArgumentNullException.ThrowIfNull(options);

        options.SupportNonNullableReferenceTypes();
        options.UseAllOfToExtendReferenceSchemas();

        options.UseAllOfForInheritance();
        options.SelectDiscriminatorNameUsing(_ => JsonApiPropertyName.Type);
        options.SelectDiscriminatorValueUsing(clrType => _resourceGraph.GetResourceType(clrType).PublicName);
        options.SelectSubTypesUsing(SelectDerivedTypes);

        options.TagActionsBy(description => GetOpenApiOperationTags(description, _controllerResourceMapping));
        options.CustomOperationIds(_operationIdSelector.GetOpenApiOperationId);
        options.CustomSchemaIds(_schemaIdSelector.GetSchemaId);

        options.OperationFilter<DocumentationOpenApiOperationFilter>();
        options.DocumentFilter<ServerDocumentFilter>();
        options.DocumentFilter<EndpointOrderingFilter>();
        options.DocumentFilter<StringEnumOrderingFilter>();
        options.DocumentFilter<SetSchemaTypeToObjectDocumentFilter>();
        options.DocumentFilter<UnusedComponentSchemaCleaner>();
    }

    private List<Type> SelectDerivedTypes(Type baseType)
    {
        if (BaseToDerivedSchemaTypes.TryGetValue(baseType, out Type? schemaOpenType))
        {
            return GetConstructedTypesFromResourceGraph(schemaOpenType);
        }

        if (baseType == typeof(AtomicOperation))
        {
            return GetConstructedTypesForAtomicOperation();
        }

        if (baseType.IsAssignableTo(typeof(IIdentifiable)))
        {
            ResourceType? resourceType = _resourceGraph.FindResourceType(baseType);

            if (resourceType != null && resourceType.IsPartOfTypeHierarchy())
            {
                return GetResourceDerivedTypes(resourceType);
            }
        }

        return [];
    }

    private List<Type> GetConstructedTypesFromResourceGraph(Type schemaOpenType)
    {
        List<Type> constructedTypes = [];

        foreach (ResourceType resourceType in _resourceGraph.GetResourceTypes())
        {
            Type constructedType = schemaOpenType.MakeGenericType(resourceType.ClrType);
            constructedTypes.Add(constructedType);
        }

        return constructedTypes;
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

    private static List<Type> GetResourceDerivedTypes(ResourceType baseType)
    {
        List<Type> clrTypes = [];
        IncludeDerivedTypes(baseType, clrTypes);
        return clrTypes;
    }

    private static void IncludeDerivedTypes(ResourceType baseType, List<Type> clrTypes)
    {
        foreach (ResourceType derivedType in baseType.DirectlyDerivedTypes)
        {
            clrTypes.Add(derivedType.ClrType);
            IncludeDerivedTypes(derivedType, clrTypes);
        }
    }

    private static IList<string> GetOpenApiOperationTags(ApiDescription description, IControllerResourceMapping controllerResourceMapping)
    {
        var actionMethod = JsonApiActionMethod.TryCreate(description.ActionDescriptor);

        switch (actionMethod)
        {
            case OperationsActionMethod:
            {
                return ["operations"];
            }
            case ResourceActionMethod resourceActionMethod:
            {
                ResourceType? resourceType = controllerResourceMapping.GetResourceTypeForController(resourceActionMethod.ControllerType);
                ConsistencyGuard.ThrowIf(resourceType == null);

                return [resourceType.PublicName];
            }
            default:
            {
                return DefaultTagsSelector(description);
            }
        }
    }
}
