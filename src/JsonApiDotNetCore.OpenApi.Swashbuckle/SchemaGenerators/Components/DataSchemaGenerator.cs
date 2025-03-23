using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;

internal sealed class DataSchemaGenerator
{
    private static readonly string[] ResourceDataPropertyNamesInOrder =
    [
        JsonApiPropertyName.Type,
        JsonApiPropertyName.Id,
        JsonApiPropertyName.Lid,
        JsonApiPropertyName.Attributes,
        JsonApiPropertyName.Relationships,
        JsonApiPropertyName.Links,
        JsonApiPropertyName.Meta
    ];

    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly GenerationCacheSchemaGenerator _generationCacheSchemaGenerator;
    private readonly ResourceTypeSchemaGenerator _resourceTypeSchemaGenerator;
    private readonly ResourceIdSchemaGenerator _resourceIdSchemaGenerator;
    private readonly ResourceIdentifierSchemaGenerator _resourceIdentifierSchemaGenerator;
    private readonly LinksVisibilitySchemaGenerator _linksVisibilitySchemaGenerator;
    private readonly IJsonApiOptions _options;
    private readonly IResourceGraph _resourceGraph;
    private readonly ResourceFieldValidationMetadataProvider _resourceFieldValidationMetadataProvider;
    private readonly RelationshipTypeFactory _relationshipTypeFactory;
    private readonly ResourceDocumentationReader _resourceDocumentationReader;

    public DataSchemaGenerator(SchemaGenerator defaultSchemaGenerator, GenerationCacheSchemaGenerator generationCacheSchemaGenerator,
        ResourceTypeSchemaGenerator resourceTypeSchemaGenerator, ResourceIdSchemaGenerator resourceIdSchemaGenerator,
        ResourceIdentifierSchemaGenerator resourceIdentifierSchemaGenerator, LinksVisibilitySchemaGenerator linksVisibilitySchemaGenerator,
        IJsonApiOptions options, IResourceGraph resourceGraph, ResourceFieldValidationMetadataProvider resourceFieldValidationMetadataProvider,
        RelationshipTypeFactory relationshipTypeFactory, ResourceDocumentationReader resourceDocumentationReader)
    {
        ArgumentNullException.ThrowIfNull(defaultSchemaGenerator);
        ArgumentNullException.ThrowIfNull(generationCacheSchemaGenerator);
        ArgumentNullException.ThrowIfNull(resourceTypeSchemaGenerator);
        ArgumentNullException.ThrowIfNull(resourceIdSchemaGenerator);
        ArgumentNullException.ThrowIfNull(resourceIdentifierSchemaGenerator);
        ArgumentNullException.ThrowIfNull(linksVisibilitySchemaGenerator);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(resourceGraph);
        ArgumentNullException.ThrowIfNull(resourceFieldValidationMetadataProvider);
        ArgumentNullException.ThrowIfNull(relationshipTypeFactory);
        ArgumentNullException.ThrowIfNull(resourceDocumentationReader);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _generationCacheSchemaGenerator = generationCacheSchemaGenerator;
        _resourceTypeSchemaGenerator = resourceTypeSchemaGenerator;
        _resourceIdSchemaGenerator = resourceIdSchemaGenerator;
        _resourceIdentifierSchemaGenerator = resourceIdentifierSchemaGenerator;
        _linksVisibilitySchemaGenerator = linksVisibilitySchemaGenerator;
        _options = options;
        _resourceGraph = resourceGraph;
        _resourceFieldValidationMetadataProvider = resourceFieldValidationMetadataProvider;
        _relationshipTypeFactory = relationshipTypeFactory;
        _resourceDocumentationReader = resourceDocumentationReader;
    }

    public OpenApiSchema GenerateSchema(Type dataSchemaType, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(dataSchemaType);
        ArgumentNullException.ThrowIfNull(schemaRepository);

        if (schemaRepository.TryLookupByType(dataSchemaType, out OpenApiSchema referenceSchemaForData))
        {
            return referenceSchemaForData;
        }

        var resourceSchemaType = ResourceSchemaType.Create(dataSchemaType, _resourceGraph);
        ResourceType resourceType = resourceSchemaType.ResourceType;

        referenceSchemaForData = _defaultSchemaGenerator.GenerateSchema(dataSchemaType, schemaRepository);
        OpenApiSchema fullSchemaForResourceData = schemaRepository.Schemas[referenceSchemaForData.Reference.Id];
        fullSchemaForResourceData.AdditionalPropertiesAllowed = false;

        OpenApiSchema inlineSchemaForResourceData = fullSchemaForResourceData.UnwrapLastExtendedSchema();
        bool isRequestSchema = inlineSchemaForResourceData == fullSchemaForResourceData;

        SetAbstract(inlineSchemaForResourceData, resourceType);
        SetResourceType(inlineSchemaForResourceData, resourceType, schemaRepository);
        AdaptResourceIdentity(inlineSchemaForResourceData, resourceSchemaType, isRequestSchema, schemaRepository);
        SetResourceId(inlineSchemaForResourceData, resourceType, schemaRepository);
        SetResourceFields(inlineSchemaForResourceData, resourceSchemaType, isRequestSchema, schemaRepository);
        SetDocumentation(fullSchemaForResourceData, resourceType);
        SetLinksVisibility(inlineSchemaForResourceData, resourceSchemaType, schemaRepository);

        inlineSchemaForResourceData.ReorderProperties(ResourceDataPropertyNamesInOrder);

        return referenceSchemaForData;
    }

    private static void SetAbstract(OpenApiSchema fullSchema, ResourceType resourceType)
    {
        if (resourceType.ClrType.IsAbstract)
        {
            fullSchema.Extensions["x-abstract"] = new OpenApiBoolean(true);
        }
    }

    private void SetResourceType(OpenApiSchema fullSchema, ResourceType resourceType, SchemaRepository schemaRepository)
    {
        if (fullSchema.Properties.ContainsKey(JsonApiPropertyName.Type))
        {
            OpenApiSchema referenceSchema = _resourceTypeSchemaGenerator.GenerateSchema(resourceType, schemaRepository);
            fullSchema.Properties[JsonApiPropertyName.Type] = referenceSchema.WrapInExtendedSchema();
        }
    }

    private void AdaptResourceIdentity(OpenApiSchema fullSchema, ResourceSchemaType resourceSchemaType, bool forRequestSchema,
        SchemaRepository schemaRepository)
    {
        if (!forRequestSchema)
        {
            return;
        }

        bool hasAtomicOperationsEndpoint = _generationCacheSchemaGenerator.HasAtomicOperationsEndpoint(schemaRepository);

        if (!hasAtomicOperationsEndpoint)
        {
            fullSchema.Properties.Remove(JsonApiPropertyName.Lid);
        }

        if (resourceSchemaType.SchemaOpenType == typeof(DataInCreateRequest<>))
        {
            ClientIdGenerationMode clientIdGeneration = resourceSchemaType.ResourceType.ClientIdGeneration ?? _options.ClientIdGeneration;

            if (hasAtomicOperationsEndpoint)
            {
                if (clientIdGeneration == ClientIdGenerationMode.Forbidden)
                {
                    fullSchema.Properties.Remove(JsonApiPropertyName.Id);
                }
                else if (clientIdGeneration == ClientIdGenerationMode.Required)
                {
                    fullSchema.Properties.Remove(JsonApiPropertyName.Lid);
                    fullSchema.Required.Add(JsonApiPropertyName.Id);
                }
            }
            else
            {
                if (clientIdGeneration == ClientIdGenerationMode.Forbidden)
                {
                    fullSchema.Properties.Remove(JsonApiPropertyName.Id);
                }
                else if (clientIdGeneration == ClientIdGenerationMode.Required)
                {
                    fullSchema.Required.Add(JsonApiPropertyName.Id);
                }
            }
        }
        else
        {
            if (!hasAtomicOperationsEndpoint)
            {
                fullSchema.Required.Add(JsonApiPropertyName.Id);
            }
        }
    }

    private void SetResourceId(OpenApiSchema fullSchema, ResourceType resourceType, SchemaRepository schemaRepository)
    {
        if (fullSchema.Properties.ContainsKey(JsonApiPropertyName.Id))
        {
            OpenApiSchema idSchema = _resourceIdSchemaGenerator.GenerateSchema(resourceType, schemaRepository);
            fullSchema.Properties[JsonApiPropertyName.Id] = idSchema;
        }
    }

    private void SetResourceFields(OpenApiSchema fullSchemaForData, ResourceSchemaType resourceSchemaType, bool forRequestSchema,
        SchemaRepository schemaRepository)
    {
        bool schemaHasFields = fullSchemaForData.Properties.ContainsKey(JsonApiPropertyName.Attributes) &&
            fullSchemaForData.Properties.ContainsKey(JsonApiPropertyName.Relationships);

        if (schemaHasFields)
        {
            var fieldSchemaBuilder = new ResourceFieldSchemaBuilder(_defaultSchemaGenerator, _resourceIdentifierSchemaGenerator,
                _linksVisibilitySchemaGenerator, _resourceFieldValidationMetadataProvider, _relationshipTypeFactory, resourceSchemaType);

            ResourceType resourceType = resourceSchemaType.ResourceType;

            SetFieldSchemaMembers(fullSchemaForData, resourceType, forRequestSchema, true, fieldSchemaBuilder, schemaRepository);
            SetFieldSchemaMembers(fullSchemaForData, resourceType, forRequestSchema, false, fieldSchemaBuilder, schemaRepository);
        }
    }

    private void SetFieldSchemaMembers(OpenApiSchema fullSchemaForData, ResourceType resourceType, bool forRequestSchema, bool forAttributes,
        ResourceFieldSchemaBuilder fieldSchemaBuilder, SchemaRepository schemaRepository)
    {
        string propertyNameInSchema = forAttributes ? JsonApiPropertyName.Attributes : JsonApiPropertyName.Relationships;

        OpenApiSchema referenceSchemaForFields = fullSchemaForData.Properties[propertyNameInSchema].UnwrapLastExtendedSchema();
        OpenApiSchema fullSchemaForFields = schemaRepository.Schemas[referenceSchemaForFields.Reference.Id];
        fullSchemaForFields.AdditionalPropertiesAllowed = false;

        SetAbstract(fullSchemaForFields, resourceType);

        if (forAttributes)
        {
            fieldSchemaBuilder.SetMembersOfAttributes(fullSchemaForFields, forRequestSchema, schemaRepository);
        }
        else
        {
            fieldSchemaBuilder.SetMembersOfRelationships(fullSchemaForFields, forRequestSchema, schemaRepository);
        }

        if (fullSchemaForFields.Properties.Count == 0)
        {
            fullSchemaForData.Properties.Remove(propertyNameInSchema);
            schemaRepository.Schemas.Remove(referenceSchemaForFields.Reference.Id);
        }
    }

    private void SetDocumentation(OpenApiSchema fullSchema, ResourceType resourceType)
    {
        fullSchema.Description = _resourceDocumentationReader.GetDocumentationForType(resourceType);
    }

    private void SetLinksVisibility(OpenApiSchema fullSchema, ResourceSchemaType resourceSchemaType, SchemaRepository schemaRepository)
    {
        _linksVisibilitySchemaGenerator.UpdateSchemaForResource(resourceSchemaType, fullSchema, schemaRepository);
    }
}
