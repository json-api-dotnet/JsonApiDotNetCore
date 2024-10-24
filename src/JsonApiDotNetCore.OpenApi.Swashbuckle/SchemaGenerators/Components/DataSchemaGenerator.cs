using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;
using JsonApiDotNetCore.Serialization.JsonConverters;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;

internal sealed class DataSchemaGenerator
{
    private const string AttributesDiscriminatorPropertyName = ResourceObjectConverter.AttributesDiscriminatorPropertyName;
    private const string RelationshipsDiscriminatorPropertyName = ResourceObjectConverter.RelationshipsDiscriminatorPropertyName;

#if NET6_0
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
#endif

    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly GenerationCacheSchemaGenerator _generationCacheSchemaGenerator;
    private readonly ResourceTypeSchemaGenerator _resourceTypeSchemaGenerator;
    private readonly ResourceIdSchemaGenerator _resourceIdSchemaGenerator;
    private readonly ResourceIdentifierSchemaGenerator _resourceIdentifierSchemaGenerator;
    private readonly LinksVisibilitySchemaGenerator _linksVisibilitySchemaGenerator;
    private readonly IResourceGraph _resourceGraph;
    private readonly IJsonApiOptions _options;
    private readonly ResourceFieldValidationMetadataProvider _resourceFieldValidationMetadataProvider;
    private readonly RelationshipTypeFactory _relationshipTypeFactory;
    private readonly ResourceDocumentationReader _resourceDocumentationReader;

    public DataSchemaGenerator(SchemaGenerator defaultSchemaGenerator, GenerationCacheSchemaGenerator generationCacheSchemaGenerator,
        ResourceTypeSchemaGenerator resourceTypeSchemaGenerator, ResourceIdSchemaGenerator resourceIdSchemaGenerator,
        ResourceIdentifierSchemaGenerator resourceIdentifierSchemaGenerator, LinksVisibilitySchemaGenerator linksVisibilitySchemaGenerator,
        IResourceGraph resourceGraph, IJsonApiOptions options, ResourceFieldValidationMetadataProvider resourceFieldValidationMetadataProvider,
        RelationshipTypeFactory relationshipTypeFactory, ResourceDocumentationReader resourceDocumentationReader)
    {
        ArgumentGuard.NotNull(defaultSchemaGenerator);
        ArgumentGuard.NotNull(generationCacheSchemaGenerator);
        ArgumentGuard.NotNull(resourceTypeSchemaGenerator);
        ArgumentGuard.NotNull(resourceIdSchemaGenerator);
        ArgumentGuard.NotNull(resourceIdentifierSchemaGenerator);
        ArgumentGuard.NotNull(linksVisibilitySchemaGenerator);
        ArgumentGuard.NotNull(resourceGraph);
        ArgumentGuard.NotNull(options);
        ArgumentGuard.NotNull(resourceFieldValidationMetadataProvider);
        ArgumentGuard.NotNull(relationshipTypeFactory);
        ArgumentGuard.NotNull(resourceDocumentationReader);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _generationCacheSchemaGenerator = generationCacheSchemaGenerator;
        _resourceTypeSchemaGenerator = resourceTypeSchemaGenerator;
        _resourceIdSchemaGenerator = resourceIdSchemaGenerator;
        _resourceIdentifierSchemaGenerator = resourceIdentifierSchemaGenerator;
        _linksVisibilitySchemaGenerator = linksVisibilitySchemaGenerator;
        _resourceGraph = resourceGraph;
        _options = options;
        _resourceFieldValidationMetadataProvider = resourceFieldValidationMetadataProvider;
        _relationshipTypeFactory = relationshipTypeFactory;
        _resourceDocumentationReader = resourceDocumentationReader;
    }

    public OpenApiSchema GenerateSchema(Type resourceDataConstructedType, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(resourceDataConstructedType);
        ArgumentGuard.NotNull(schemaRepository);

        // TODO: Rename referenceSchemaForResourceData to referenceSchemaForData?
        if (schemaRepository.TryLookupByType(resourceDataConstructedType, out OpenApiSchema referenceSchemaForResourceData))
        {
            return referenceSchemaForResourceData;
        }

        var resourceTypeInfo = ResourceTypeInfo.Create(resourceDataConstructedType, _resourceGraph);
        OpenApiSchema? referenceSchemaForBaseResourceData = null;

        if (resourceTypeInfo.ResourceType.BaseType != null)
        {
            Type baseResourceDataConstructedType =
                resourceDataConstructedType.GetGenericTypeDefinition().MakeGenericType(resourceTypeInfo.ResourceType.BaseType.ClrType);

            referenceSchemaForBaseResourceData = GenerateSchema(baseResourceDataConstructedType, schemaRepository);
        }

        referenceSchemaForResourceData = _defaultSchemaGenerator.GenerateSchema(resourceDataConstructedType, schemaRepository);
        OpenApiSchema fullSchemaForResourceData = schemaRepository.Schemas[referenceSchemaForResourceData.Reference.Id];
        fullSchemaForResourceData.AdditionalPropertiesAllowed = false;

        OpenApiSchema fullSchemaForDerivedType = fullSchemaForResourceData.UnwrapLastExtendedSchema();
        bool isRequestSchema = fullSchemaForDerivedType == fullSchemaForResourceData;

        if (isRequestSchema)
        {
            bool hasAtomicOperationsEndpoint = _generationCacheSchemaGenerator.HasAtomicOperationsEndpoint(schemaRepository);
            AdaptResourceIdentity(resourceTypeInfo, fullSchemaForResourceData, hasAtomicOperationsEndpoint);
            SetResourceType(fullSchemaForResourceData, resourceTypeInfo.ResourceType, schemaRepository);
        }

        SetResourceId(fullSchemaForDerivedType, resourceTypeInfo.ResourceType, schemaRepository);

        fullSchemaForResourceData.Description = _resourceDocumentationReader.GetDocumentationForType(resourceTypeInfo.ResourceType);

        var fieldSchemaBuilder = new ResourceFieldSchemaBuilder(_defaultSchemaGenerator, _resourceIdentifierSchemaGenerator, _linksVisibilitySchemaGenerator,
            _resourceFieldValidationMetadataProvider, _relationshipTypeFactory, resourceTypeInfo);

        if (fullSchemaForDerivedType.Properties.ContainsKey(JsonApiPropertyName.Attributes))
        {
            SetResourceAttributes(fullSchemaForDerivedType, resourceTypeInfo.ResourceType, isRequestSchema, fieldSchemaBuilder,
                referenceSchemaForBaseResourceData, schemaRepository);
        }

        if (fullSchemaForDerivedType.Properties.ContainsKey(JsonApiPropertyName.Relationships))
        {
            SetResourceRelationships(fullSchemaForDerivedType, resourceTypeInfo.ResourceType, isRequestSchema, fieldSchemaBuilder,
                referenceSchemaForBaseResourceData, schemaRepository);
        }

        if (!isRequestSchema && referenceSchemaForBaseResourceData != null)
        {
            fullSchemaForResourceData.AllOf[0] = referenceSchemaForBaseResourceData;
        }

        if (isRequestSchema && resourceTypeInfo.ResourceType.IsPartOfTypeHierarchy() && resourceTypeInfo.ResourceType.BaseType != null)
        {
            MapDataInDiscriminator(resourceDataConstructedType, referenceSchemaForResourceData.Reference.ReferenceV3, resourceTypeInfo.ResourceType,
                schemaRepository);

            var compositeFullSchema = new OpenApiSchema
            {
                AllOf =
                [
                    referenceSchemaForBaseResourceData,
                    fullSchemaForDerivedType
                ],
                AdditionalPropertiesAllowed = false // TODO: Do we need this?
            };

            schemaRepository.Schemas[referenceSchemaForResourceData.Reference.Id] = compositeFullSchema;
        }

        _linksVisibilitySchemaGenerator.UpdateSchemaForResource(resourceTypeInfo, fullSchemaForDerivedType, schemaRepository);

#if NET6_0
        fullSchemaForDerivedType.ReorderProperties(ResourceDataPropertyNamesInOrder);
#endif

        return referenceSchemaForResourceData;
    }

    private void MapDataInDiscriminator(Type resourceDataConstructedType, string discriminatorValue, ResourceType resourceType,
        SchemaRepository schemaRepository)
    {
        ResourceType ultimateBaseResourceType = resourceType.GetUltimateBaseType();
        Type resourceDataOpenType = resourceDataConstructedType.ConstructedToOpenType();
        Type ultimateBaseSchemaType = resourceDataOpenType.MakeGenericType(ultimateBaseResourceType.ClrType);

        if (!schemaRepository.TryLookupByType(ultimateBaseSchemaType, out OpenApiSchema? referenceSchema))
        {
            throw new UnreachableCodeException();
        }

        OpenApiSchema? fullSchema = schemaRepository.Schemas[referenceSchema.Reference.Id];

        fullSchema.Discriminator ??= new OpenApiDiscriminator
        {
            PropertyName = "type",
            Mapping = new SortedDictionary<string, string>(StringComparer.Ordinal)
        };

        fullSchema.Discriminator.Mapping.Add(resourceType.PublicName, discriminatorValue);
    }

    private void AdaptResourceIdentity(ResourceTypeInfo resourceTypeInfo, OpenApiSchema fullSchemaForResourceData, bool hasAtomicOperationsEndpoint)
    {
        if (!hasAtomicOperationsEndpoint)
        {
            fullSchemaForResourceData.Properties.Remove(JsonApiPropertyName.Lid);
        }

        if (resourceTypeInfo.ResourceDataOpenType == typeof(DataInCreateResourceRequest<>))
        {
            ClientIdGenerationMode clientIdGeneration = resourceTypeInfo.ResourceType.ClientIdGeneration ?? _options.ClientIdGeneration;

            if (hasAtomicOperationsEndpoint)
            {
                if (clientIdGeneration == ClientIdGenerationMode.Forbidden)
                {
                    fullSchemaForResourceData.Properties.Remove(JsonApiPropertyName.Id);
                }
                else if (clientIdGeneration == ClientIdGenerationMode.Required)
                {
                    fullSchemaForResourceData.Properties.Remove(JsonApiPropertyName.Lid);
                    fullSchemaForResourceData.Required.Add(JsonApiPropertyName.Id);
                }
            }
            else
            {
                if (clientIdGeneration == ClientIdGenerationMode.Forbidden)
                {
                    fullSchemaForResourceData.Properties.Remove(JsonApiPropertyName.Id);
                }
                else if (clientIdGeneration == ClientIdGenerationMode.Required)
                {
                    fullSchemaForResourceData.Required.Add(JsonApiPropertyName.Id);
                }
            }
        }
        else
        {
            if (!hasAtomicOperationsEndpoint)
            {
                fullSchemaForResourceData.Required.Add(JsonApiPropertyName.Id);
            }
        }
    }

    private void SetResourceType(OpenApiSchema fullSchemaForResourceData, ResourceType resourceType, SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchema = _resourceTypeSchemaGenerator.GenerateSchema(resourceType, schemaRepository);
        fullSchemaForResourceData.Properties[JsonApiPropertyName.Type] = referenceSchema.WrapInExtendedSchema();
    }

    private void SetResourceId(OpenApiSchema fullSchemaForResourceData, ResourceType resourceType, SchemaRepository schemaRepository)
    {
        if (fullSchemaForResourceData.Properties.ContainsKey(JsonApiPropertyName.Id))
        {
            OpenApiSchema idSchema = _resourceIdSchemaGenerator.GenerateSchema(resourceType, schemaRepository);
            fullSchemaForResourceData.Properties[JsonApiPropertyName.Id] = idSchema;
        }
    }

    private OpenApiSchema SetResourceAttributes(OpenApiSchema fullSchemaForResourceData, ResourceType resourceType, bool forRequestSchema,
        ResourceFieldSchemaBuilder builder, OpenApiSchema? referenceSchemaForBaseResourceData, SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchemaForAttributes =
            fullSchemaForResourceData.UnwrapLastExtendedSchema().Properties[JsonApiPropertyName.Attributes].UnwrapLastExtendedSchema();

        OpenApiSchema fullSchemaForAttributes = schemaRepository.Schemas[referenceSchemaForAttributes.Reference.Id];

        if (fullSchemaForAttributes.UnwrapLastExtendedSchema().Properties.Count > 0)
        {
            // already generated
            return referenceSchemaForAttributes;
        }

        OpenApiSchema? referenceSchemaForBaseAttributes = null;

        // TODO: Handle DataInCreateResourceRequest<>
        if (!forRequestSchema && resourceType.BaseType != null && referenceSchemaForBaseResourceData != null)
        {
            OpenApiSchema? fullSchemaForBaseResourceData = schemaRepository.Schemas[referenceSchemaForBaseResourceData.Reference.Id];
            Type baseResourceDataConstructedType = typeof(ResourceDataInResponse<>).MakeGenericType(resourceType.BaseType.ClrType);

            var baseResourceTypeInfo = ResourceTypeInfo.Create(baseResourceDataConstructedType, _resourceGraph);

            var baseFieldSchemaBuilder = new ResourceFieldSchemaBuilder(_defaultSchemaGenerator, _resourceIdentifierSchemaGenerator,
                _linksVisibilitySchemaGenerator, _resourceFieldValidationMetadataProvider, _relationshipTypeFactory, baseResourceTypeInfo);

            referenceSchemaForBaseAttributes = SetResourceAttributes(fullSchemaForBaseResourceData, resourceType.BaseType, forRequestSchema,
                baseFieldSchemaBuilder, null, schemaRepository);

            if (resourceType.BaseType.BaseType == null)
            {
                // generate ultimate base with discriminator and empty mapping
                OpenApiSchema? fullSchemaForBaseAttributes = schemaRepository.Schemas[referenceSchemaForBaseAttributes.Reference.Id];

                if (fullSchemaForBaseAttributes.Properties.ContainsKey(AttributesDiscriminatorPropertyName))
                {
                    // TODO: This should run only once for both attributes and relationships.
                }
                else
                {
                    fullSchemaForBaseAttributes.Properties.Add(AttributesDiscriminatorPropertyName, new OpenApiSchema
                    {
                        Type = "string"
                    });

                    fullSchemaForBaseAttributes.Required.Add(AttributesDiscriminatorPropertyName);

                    fullSchemaForBaseAttributes.Discriminator = new OpenApiDiscriminator
                    {
                        PropertyName = AttributesDiscriminatorPropertyName,
                        Mapping = new SortedDictionary<string, string>(StringComparer.Ordinal)
                    };

                    if (resourceType.BaseType.ClrType.IsAbstract)
                    {
                        fullSchemaForBaseAttributes.Extensions = new Dictionary<string, IOpenApiExtension>
                        {
                            ["x-abstract"] = new OpenApiBoolean(true)
                        };
                    }
                }
            }
        }

        builder.SetMembersOfAttributes(fullSchemaForAttributes, forRequestSchema, schemaRepository);

        if (fullSchemaForAttributes.Properties.Count == 0 && (!builder.ResourceType.IsPartOfTypeHierarchy() || forRequestSchema))
        {
            fullSchemaForResourceData.Properties.Remove(JsonApiPropertyName.Attributes);
        }
        else
        {
            fullSchemaForAttributes.AdditionalPropertiesAllowed = false;
        }

        if (referenceSchemaForBaseAttributes != null)
        {
            var newFullSchemaForAttributes = new OpenApiSchema
            {
                AllOf =
                [
                    referenceSchemaForBaseAttributes,
                    fullSchemaForAttributes
                ],
                AdditionalPropertiesAllowed = false
            };

            schemaRepository.Schemas[referenceSchemaForAttributes.Reference.Id] = newFullSchemaForAttributes;

            MapAttributesInDiscriminator(resourceType, referenceSchemaForAttributes, schemaRepository);
        }

        return referenceSchemaForAttributes;
    }

    private static void MapAttributesInDiscriminator(ResourceType resourceType, OpenApiSchema referenceSchemaForAttributes, SchemaRepository schemaRepository)
    {
        ResourceType ultimateBaseResourceType = resourceType.GetUltimateBaseType();
        Type ultimateConstructedBaseType = typeof(AttributesInResponse<>).MakeGenericType(ultimateBaseResourceType.ClrType);

        if (!schemaRepository.TryLookupByType(ultimateConstructedBaseType, out OpenApiSchema? referenceSchemaForUltimateBaseAttributes))
        {
            throw new UnreachableCodeException();
        }

        OpenApiSchema fullSchemaForUltimateBaseAttributes = schemaRepository.Schemas[referenceSchemaForUltimateBaseAttributes.Reference.Id];
        fullSchemaForUltimateBaseAttributes.Discriminator.Mapping[resourceType.PublicName] = referenceSchemaForAttributes.Reference.ReferenceV3;
    }

    private OpenApiSchema SetResourceRelationships(OpenApiSchema fullSchemaForResourceData, ResourceType resourceType, bool forRequestSchema,
        ResourceFieldSchemaBuilder builder, OpenApiSchema? referenceSchemaForBaseResourceData, SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchemaForRelationships =
            fullSchemaForResourceData.UnwrapLastExtendedSchema().Properties[JsonApiPropertyName.Relationships].UnwrapLastExtendedSchema();

        OpenApiSchema fullSchemaForRelationships = schemaRepository.Schemas[referenceSchemaForRelationships.Reference.Id];

        if (fullSchemaForRelationships.UnwrapLastExtendedSchema().Properties.Count > 0)
        {
            // already generated
            return referenceSchemaForRelationships;
        }

        OpenApiSchema? referenceSchemaForBaseRelationships = null;

        // TODO: Handle DataInCreateResourceRequest<>
        if (!forRequestSchema && resourceType.BaseType != null && referenceSchemaForBaseResourceData != null)
        {
            OpenApiSchema? fullSchemaForBaseResourceData = schemaRepository.Schemas[referenceSchemaForBaseResourceData.Reference.Id];
            Type baseResourceDataConstructedType = typeof(ResourceDataInResponse<>).MakeGenericType(resourceType.BaseType.ClrType);

            var baseResourceTypeInfo = ResourceTypeInfo.Create(baseResourceDataConstructedType, _resourceGraph);

            var baseFieldSchemaBuilder = new ResourceFieldSchemaBuilder(_defaultSchemaGenerator, _resourceIdentifierSchemaGenerator,
                _linksVisibilitySchemaGenerator, _resourceFieldValidationMetadataProvider, _relationshipTypeFactory, baseResourceTypeInfo);

            referenceSchemaForBaseRelationships = SetResourceRelationships(fullSchemaForBaseResourceData, resourceType.BaseType, forRequestSchema,
                baseFieldSchemaBuilder, null, schemaRepository);

            if (resourceType.BaseType.BaseType == null)
            {
                // generate ultimate base with discriminator and empty mapping
                OpenApiSchema? fullSchemaForBaseRelationships = schemaRepository.Schemas[referenceSchemaForBaseRelationships.Reference.Id];

                if (fullSchemaForBaseRelationships.Properties.ContainsKey(RelationshipsDiscriminatorPropertyName))
                {
                    // TODO: This should run only once for both attributes and relationships.
                }
                else
                {
                    fullSchemaForBaseRelationships.Properties.Add(RelationshipsDiscriminatorPropertyName, new OpenApiSchema
                    {
                        Type = "string"
                    });

                    fullSchemaForBaseRelationships.Required.Add(RelationshipsDiscriminatorPropertyName);

                    fullSchemaForBaseRelationships.Discriminator = new OpenApiDiscriminator
                    {
                        PropertyName = RelationshipsDiscriminatorPropertyName,
                        Mapping = new SortedDictionary<string, string>(StringComparer.Ordinal)
                    };

                    if (resourceType.BaseType.ClrType.IsAbstract)
                    {
                        fullSchemaForBaseRelationships.Extensions = new Dictionary<string, IOpenApiExtension>
                        {
                            ["x-abstract"] = new OpenApiBoolean(true)
                        };
                    }
                }
            }
        }

        builder.SetMembersOfRelationships(fullSchemaForRelationships, forRequestSchema, schemaRepository);

        if (fullSchemaForRelationships.Properties.Count == 0 && (!builder.ResourceType.IsPartOfTypeHierarchy() || forRequestSchema))
        {
            fullSchemaForResourceData.Properties.Remove(JsonApiPropertyName.Relationships);
        }
        else
        {
            fullSchemaForRelationships.AdditionalPropertiesAllowed = false;
        }

        if (referenceSchemaForBaseRelationships != null)
        {
            var newFullSchemaForRelationships = new OpenApiSchema
            {
                AllOf =
                [
                    referenceSchemaForBaseRelationships,
                    fullSchemaForRelationships
                ],
                AdditionalPropertiesAllowed = false
            };

            schemaRepository.Schemas[referenceSchemaForRelationships.Reference.Id] = newFullSchemaForRelationships;

            MapRelationshipsInDiscriminator(resourceType, referenceSchemaForRelationships, schemaRepository);
        }

        return referenceSchemaForRelationships;
    }

    private static void MapRelationshipsInDiscriminator(ResourceType resourceType, OpenApiSchema referenceSchemaForRelationships,
        SchemaRepository schemaRepository)
    {
        ResourceType ultimateBaseResourceType = resourceType.GetUltimateBaseType();
        Type ultimateConstructedBaseType = typeof(RelationshipsInResponse<>).MakeGenericType(ultimateBaseResourceType.ClrType);

        if (!schemaRepository.TryLookupByType(ultimateConstructedBaseType, out OpenApiSchema? referenceSchemaForUltimateBaseRelationships))
        {
            throw new UnreachableCodeException();
        }

        OpenApiSchema fullSchemaForUltimateBaseRelationships = schemaRepository.Schemas[referenceSchemaForUltimateBaseRelationships.Reference.Id];
        fullSchemaForUltimateBaseRelationships.Discriminator.Mapping[resourceType.PublicName] = referenceSchemaForRelationships.Reference.ReferenceV3;
    }
}
