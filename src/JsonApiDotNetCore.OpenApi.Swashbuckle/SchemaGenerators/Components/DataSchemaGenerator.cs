using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
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
    // Workaround for bug at https://github.com/microsoft/kiota/issues/2432#issuecomment-2436625836.
    private static readonly bool RepeatDiscriminatorInResponseDerivedTypes = bool.Parse(bool.TrueString);

    private static readonly ConcurrentDictionary<ResourceType, ResourceType> UltimateBaseResourceTypeCache = [];

    private static readonly string[] DataPropertyNamesInOrder =
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
    private readonly LinksVisibilitySchemaGenerator _linksVisibilitySchemaGenerator;
    private readonly MetaSchemaGenerator _metaSchemaGenerator;
    private readonly JsonApiSchemaIdSelector _schemaIdSelector;
    private readonly IJsonApiOptions _options;
    private readonly IResourceGraph _resourceGraph;
    private readonly ResourceFieldValidationMetadataProvider _resourceFieldValidationMetadataProvider;
    private readonly RelationshipTypeFactory _relationshipTypeFactory;
    private readonly ResourceDocumentationReader _resourceDocumentationReader;

    public DataSchemaGenerator(SchemaGenerator defaultSchemaGenerator, GenerationCacheSchemaGenerator generationCacheSchemaGenerator,
        ResourceTypeSchemaGenerator resourceTypeSchemaGenerator, ResourceIdSchemaGenerator resourceIdSchemaGenerator,
        LinksVisibilitySchemaGenerator linksVisibilitySchemaGenerator, MetaSchemaGenerator metaSchemaGenerator, JsonApiSchemaIdSelector schemaIdSelector,
        IJsonApiOptions options, IResourceGraph resourceGraph, ResourceFieldValidationMetadataProvider resourceFieldValidationMetadataProvider,
        RelationshipTypeFactory relationshipTypeFactory, ResourceDocumentationReader resourceDocumentationReader)
    {
        ArgumentNullException.ThrowIfNull(defaultSchemaGenerator);
        ArgumentNullException.ThrowIfNull(generationCacheSchemaGenerator);
        ArgumentNullException.ThrowIfNull(resourceTypeSchemaGenerator);
        ArgumentNullException.ThrowIfNull(resourceIdSchemaGenerator);
        ArgumentNullException.ThrowIfNull(linksVisibilitySchemaGenerator);
        ArgumentNullException.ThrowIfNull(metaSchemaGenerator);
        ArgumentNullException.ThrowIfNull(schemaIdSelector);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(resourceGraph);
        ArgumentNullException.ThrowIfNull(resourceFieldValidationMetadataProvider);
        ArgumentNullException.ThrowIfNull(relationshipTypeFactory);
        ArgumentNullException.ThrowIfNull(resourceDocumentationReader);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _generationCacheSchemaGenerator = generationCacheSchemaGenerator;
        _resourceTypeSchemaGenerator = resourceTypeSchemaGenerator;
        _resourceIdSchemaGenerator = resourceIdSchemaGenerator;
        _linksVisibilitySchemaGenerator = linksVisibilitySchemaGenerator;
        _metaSchemaGenerator = metaSchemaGenerator;
        _schemaIdSelector = schemaIdSelector;
        _options = options;
        _resourceGraph = resourceGraph;
        _resourceFieldValidationMetadataProvider = resourceFieldValidationMetadataProvider;
        _relationshipTypeFactory = relationshipTypeFactory;
        _resourceDocumentationReader = resourceDocumentationReader;
    }

    public OpenApiSchema GenerateSchema(Type dataSchemaType, bool forRequestSchema, SchemaRepository schemaRepository)
    {
        // For a given resource (identifier) type, we always generate the full type hierarchy. Discriminator mappings
        // are managed manually, because there's no way to intercept in the Swashbuckle recursive component schema generation.

        ArgumentNullException.ThrowIfNull(dataSchemaType);
        ArgumentNullException.ThrowIfNull(schemaRepository);

        if (schemaRepository.TryLookupByType(dataSchemaType, out OpenApiSchema referenceSchemaForData))
        {
            return referenceSchemaForData;
        }

        var resourceSchemaType = ResourceSchemaType.Create(dataSchemaType, _resourceGraph);
        ResourceType resourceType = resourceSchemaType.ResourceType;

        Type? commonDataSchemaType = GetCommonSchemaType(resourceSchemaType.SchemaOpenType);

        if (commonDataSchemaType != null)
        {
            _ = GenerateSchemaForCommonData(commonDataSchemaType, schemaRepository);
        }

        if (resourceType.BaseType != null)
        {
            ResourceType ultimateBaseResourceType = GetUltimateBaseType(resourceType);
            Type ultimateBaseSchemaType = ChangeResourceTypeInSchemaType(dataSchemaType, ultimateBaseResourceType);

            _ = GenerateSchema(ultimateBaseSchemaType, forRequestSchema, schemaRepository);

            return schemaRepository.LookupByType(dataSchemaType);
        }

        referenceSchemaForData = _defaultSchemaGenerator.GenerateSchema(dataSchemaType, schemaRepository);
        OpenApiSchema fullSchemaForData = schemaRepository.Schemas[referenceSchemaForData.Reference.Id];
        fullSchemaForData.AdditionalPropertiesAllowed = false;

        OpenApiSchema inlineSchemaForData = fullSchemaForData.UnwrapLastExtendedSchema();

        SetAbstract(inlineSchemaForData, resourceSchemaType);
        SetResourceType(inlineSchemaForData, resourceType, schemaRepository);
        AdaptResourceIdentity(inlineSchemaForData, resourceSchemaType, forRequestSchema, schemaRepository);
        SetResourceId(inlineSchemaForData, resourceType, schemaRepository);
        SetResourceFields(inlineSchemaForData, resourceSchemaType, forRequestSchema, schemaRepository);
        SetDocumentation(fullSchemaForData, resourceType);
        SetLinksVisibility(inlineSchemaForData, resourceSchemaType, schemaRepository);

        if (resourceType.IsPartOfTypeHierarchy())
        {
            GenerateDataSchemasForDirectlyDerivedTypes(resourceSchemaType, forRequestSchema, schemaRepository);
        }

        inlineSchemaForData.ReorderProperties(DataPropertyNamesInOrder);

        if (commonDataSchemaType != null)
        {
            MapInDiscriminator(resourceSchemaType, forRequestSchema, JsonApiPropertyName.Type, schemaRepository);
        }

        if (RequiresRootObjectTypeInDataSchema(resourceSchemaType, forRequestSchema))
        {
            fullSchemaForData.Extensions[SetSchemaTypeToObjectDocumentFilter.RequiresRootObjectTypeKey] = new OpenApiBoolean(true);
        }

        return referenceSchemaForData;
    }

    private static Type? GetCommonSchemaType(Type schemaOpenType)
    {
        if (schemaOpenType == typeof(IdentifierInRequest<>))
        {
            return typeof(IdentifierInRequest);
        }

        if (schemaOpenType == typeof(DataInCreateRequest<>))
        {
            return typeof(ResourceInCreateRequest);
        }

        if (schemaOpenType == typeof(AttributesInCreateRequest<>))
        {
            return typeof(AttributesInCreateRequest);
        }

        if (schemaOpenType == typeof(RelationshipsInCreateRequest<>))
        {
            return typeof(RelationshipsInCreateRequest);
        }

        if (schemaOpenType == typeof(DataInUpdateRequest<>))
        {
            return typeof(ResourceInUpdateRequest);
        }

        if (schemaOpenType == typeof(AttributesInUpdateRequest<>))
        {
            return typeof(AttributesInUpdateRequest);
        }

        if (schemaOpenType == typeof(RelationshipsInUpdateRequest<>))
        {
            return typeof(RelationshipsInUpdateRequest);
        }

        if (schemaOpenType == typeof(IdentifierInResponse<>))
        {
            return null;
        }

        if (schemaOpenType == typeof(DataInResponse<>))
        {
            return typeof(ResourceInResponse);
        }

        if (schemaOpenType == typeof(AttributesInResponse<>))
        {
            return typeof(AttributesInResponse);
        }

        if (schemaOpenType == typeof(RelationshipsInResponse<>))
        {
            return typeof(RelationshipsInResponse);
        }

        throw new UnreachableException();
    }

    public OpenApiSchema GenerateSchemaForCommonData(Type commonDataSchemaType, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(commonDataSchemaType);
        ArgumentNullException.ThrowIfNull(schemaRepository);

        if (schemaRepository.TryLookupByType(commonDataSchemaType, out OpenApiSchema? referenceSchema))
        {
            return referenceSchema;
        }

        OpenApiSchema referenceSchemaForResourceType = _resourceTypeSchemaGenerator.GenerateSchema(schemaRepository);
        OpenApiSchema referenceSchemaForMeta = _metaSchemaGenerator.GenerateSchema(schemaRepository);

        var fullSchema = new OpenApiSchema
        {
            Type = "object",
            Required = new SortedSet<string>([JsonApiPropertyName.Type]),
            Properties = new Dictionary<string, OpenApiSchema>
            {
                [JsonApiPropertyName.Type] = referenceSchemaForResourceType.WrapInExtendedSchema(),
                [referenceSchemaForMeta.Reference.Id] = referenceSchemaForMeta.WrapInExtendedSchema()
            },
            AdditionalPropertiesAllowed = false,
            Discriminator = new OpenApiDiscriminator
            {
                PropertyName = JsonApiPropertyName.Type,
                Mapping = new SortedDictionary<string, string>(StringComparer.Ordinal)
            },
            Extensions =
            {
                ["x-abstract"] = new OpenApiBoolean(true)
            }
        };

        string schemaId = _schemaIdSelector.GetSchemaId(commonDataSchemaType);

        referenceSchema = schemaRepository.AddDefinition(schemaId, fullSchema);
        schemaRepository.RegisterType(commonDataSchemaType, schemaId);

        return referenceSchema;
    }

    private static ResourceType GetUltimateBaseType(ResourceType resourceType)
    {
        return UltimateBaseResourceTypeCache.GetOrAdd(resourceType, type =>
        {
            ResourceType baseType = type;

            while (baseType.BaseType != null)
            {
                baseType = baseType.BaseType;
            }

            return baseType;
        });
    }

    private static Type ChangeResourceTypeInSchemaType(Type schemaType, ResourceType resourceType)
    {
        Type schemaOpenType = schemaType.ConstructedToOpenType();
        return schemaOpenType.MakeGenericType(resourceType.ClrType);
    }

    private static void SetAbstract(OpenApiSchema fullSchema, ResourceSchemaType resourceSchemaType)
    {
        if (resourceSchemaType.ResourceType.ClrType.IsAbstract && resourceSchemaType.SchemaOpenType != typeof(IdentifierInRequest<>))
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
            var fieldSchemaBuilder = new ResourceFieldSchemaBuilder(_defaultSchemaGenerator, this, _linksVisibilitySchemaGenerator,
                _resourceFieldValidationMetadataProvider, _relationshipTypeFactory, resourceSchemaType);

            SetFieldSchemaMembers(fullSchemaForData, resourceSchemaType, forRequestSchema, true, fieldSchemaBuilder, schemaRepository);
            SetFieldSchemaMembers(fullSchemaForData, resourceSchemaType, forRequestSchema, false, fieldSchemaBuilder, schemaRepository);
        }
    }

    private void SetFieldSchemaMembers(OpenApiSchema fullSchemaForData, ResourceSchemaType resourceSchemaTypeForData, bool forRequestSchema, bool forAttributes,
        ResourceFieldSchemaBuilder fieldSchemaBuilder, SchemaRepository schemaRepository)
    {
        string propertyNameInSchema = forAttributes ? JsonApiPropertyName.Attributes : JsonApiPropertyName.Relationships;

        OpenApiSchema referenceSchemaForFields = fullSchemaForData.Properties[propertyNameInSchema].UnwrapLastExtendedSchema();
        OpenApiSchema fullSchemaForFields = schemaRepository.Schemas[referenceSchemaForFields.Reference.Id];
        fullSchemaForFields.AdditionalPropertiesAllowed = false;

        SetAbstract(fullSchemaForFields, resourceSchemaTypeForData);

        if (forAttributes)
        {
            fieldSchemaBuilder.SetMembersOfAttributes(fullSchemaForFields, forRequestSchema, schemaRepository);
        }
        else
        {
            fieldSchemaBuilder.SetMembersOfRelationships(fullSchemaForFields, forRequestSchema, schemaRepository);
        }

        if (fullSchemaForFields.Properties.Count == 0 && !resourceSchemaTypeForData.ResourceType.IsPartOfTypeHierarchy())
        {
            fullSchemaForData.Properties.Remove(propertyNameInSchema);
            schemaRepository.Schemas.Remove(referenceSchemaForFields.Reference.Id);
        }
        else
        {
            ResourceSchemaType resourceSchemaTypeForFields =
                GetResourceSchemaTypeForFieldsProperty(resourceSchemaTypeForData, forAttributes ? "Attributes" : "Relationships");

            Type? commonFieldsSchemaType = GetCommonSchemaType(resourceSchemaTypeForFields.SchemaOpenType);
            ConsistencyGuard.ThrowIf(commonFieldsSchemaType == null);

            _ = GenerateSchemaForCommonFields(commonFieldsSchemaType, schemaRepository);

            MapInDiscriminator(resourceSchemaTypeForFields, forRequestSchema, OpenApiMediaTypeExtension.FullyQualifiedOpenApiDiscriminatorPropertyName,
                schemaRepository);

            Type baseSchemaType;

            if (resourceSchemaTypeForFields.ResourceType.BaseType != null)
            {
                ResourceSchemaType resourceSchemaTypeForBase =
                    resourceSchemaTypeForFields.ChangeResourceType(resourceSchemaTypeForFields.ResourceType.BaseType);

                baseSchemaType = resourceSchemaTypeForBase.SchemaConstructedType;
            }
            else
            {
                baseSchemaType = commonFieldsSchemaType;
            }

            OpenApiSchema referenceSchemaForBase = schemaRepository.LookupByType(baseSchemaType);

            schemaRepository.Schemas[referenceSchemaForFields.Reference.Id] = new OpenApiSchema
            {
                AllOf =
                [
                    referenceSchemaForBase,
                    fullSchemaForFields
                ],
                AdditionalPropertiesAllowed = false
            };
        }
    }

    private ResourceSchemaType GetResourceSchemaTypeForFieldsProperty(ResourceSchemaType resourceSchemaTypeForData, string propertyName)
    {
        PropertyInfo? fieldsProperty = resourceSchemaTypeForData.SchemaConstructedType.GetProperty(propertyName);
        ConsistencyGuard.ThrowIf(fieldsProperty == null);

        Type fieldsConstructedType = fieldsProperty.PropertyType;
        return ResourceSchemaType.Create(fieldsConstructedType, _resourceGraph);
    }

    private OpenApiSchema GenerateSchemaForCommonFields(Type commonFieldsSchemaType, SchemaRepository schemaRepository)
    {
        if (schemaRepository.TryLookupByType(commonFieldsSchemaType, out OpenApiSchema? referenceSchema))
        {
            return referenceSchema;
        }

        OpenApiSchema referenceSchemaForResourceType = _resourceTypeSchemaGenerator.GenerateSchema(schemaRepository);

        var fullSchema = new OpenApiSchema
        {
            Type = "object",
            Required = new SortedSet<string>([OpenApiMediaTypeExtension.FullyQualifiedOpenApiDiscriminatorPropertyName]),
            Properties = new Dictionary<string, OpenApiSchema>
            {
                [OpenApiMediaTypeExtension.FullyQualifiedOpenApiDiscriminatorPropertyName] = referenceSchemaForResourceType.WrapInExtendedSchema()
            },
            AdditionalPropertiesAllowed = false,
            Discriminator = new OpenApiDiscriminator
            {
                PropertyName = OpenApiMediaTypeExtension.FullyQualifiedOpenApiDiscriminatorPropertyName,
                Mapping = new SortedDictionary<string, string>(StringComparer.Ordinal)
            },
            Extensions =
            {
                ["x-abstract"] = new OpenApiBoolean(true)
            }
        };

        string schemaId = _schemaIdSelector.GetSchemaId(commonFieldsSchemaType);

        referenceSchema = schemaRepository.AddDefinition(schemaId, fullSchema);
        schemaRepository.RegisterType(commonFieldsSchemaType, schemaId);

        return referenceSchema;
    }

    private void MapInDiscriminator(ResourceSchemaType resourceSchemaType, bool forRequestSchema, string discriminatorPropertyName,
        SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchemaForDerived = schemaRepository.LookupByType(resourceSchemaType.SchemaConstructedType);

        foreach (ResourceType? baseResourceType in GetBaseTypesToMapInto(resourceSchemaType, forRequestSchema))
        {
            Type baseSchemaType = baseResourceType == null
                ? GetCommonSchemaType(resourceSchemaType.SchemaOpenType)!
                : resourceSchemaType.ChangeResourceType(baseResourceType).SchemaConstructedType;

            OpenApiSchema referenceSchemaForBase = schemaRepository.LookupByType(baseSchemaType);
            OpenApiSchema inlineSchemaForBase = schemaRepository.Schemas[referenceSchemaForBase.Reference.Id].UnwrapLastExtendedSchema();

            inlineSchemaForBase.Discriminator ??= new OpenApiDiscriminator
            {
                PropertyName = discriminatorPropertyName,
                Mapping = new SortedDictionary<string, string>(StringComparer.Ordinal)
            };

            if (RepeatDiscriminatorInResponseDerivedTypes && !forRequestSchema)
            {
                inlineSchemaForBase.Required.Add(discriminatorPropertyName);
            }

            string publicName = resourceSchemaType.ResourceType.PublicName;

            if (inlineSchemaForBase.Discriminator.Mapping.TryAdd(publicName, referenceSchemaForDerived.Reference.ReferenceV3) && baseResourceType == null)
            {
                MapResourceTypeInEnum(publicName, schemaRepository);
            }
        }
    }

    private static IEnumerable<ResourceType?> GetBaseTypesToMapInto(ResourceSchemaType resourceSchemaType, bool forRequestSchema)
    {
        bool dependsOnCommonSchemaType = GetCommonSchemaType(resourceSchemaType.SchemaOpenType) != null;

        if (RepeatDiscriminatorInResponseDerivedTypes && !forRequestSchema)
        {
            ResourceType? baseType = resourceSchemaType.ResourceType.BaseType;

            while (baseType != null)
            {
                yield return baseType;

                baseType = baseType.BaseType;
            }
        }
        else
        {
            if (!dependsOnCommonSchemaType)
            {
                yield return GetUltimateBaseType(resourceSchemaType.ResourceType);
            }
        }

        if (dependsOnCommonSchemaType)
        {
            yield return null;
        }
    }

    private void MapResourceTypeInEnum(string publicName, SchemaRepository schemaRepository)
    {
        string schemaId = _schemaIdSelector.GetResourceTypeSchemaId(null);
        OpenApiSchema fullSchema = schemaRepository.Schemas[schemaId];

        if (!fullSchema.Enum.Any(openApiAny => openApiAny is OpenApiString openApiString && openApiString.Value == publicName))
        {
            fullSchema.Enum.Add(new OpenApiString(publicName));
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

    private void GenerateDataSchemasForDirectlyDerivedTypes(ResourceSchemaType resourceSchemaType, bool forRequestSchema, SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchemaForBase = schemaRepository.LookupByType(resourceSchemaType.SchemaConstructedType);

        foreach (ResourceType derivedType in resourceSchemaType.ResourceType.DirectlyDerivedTypes)
        {
            ResourceSchemaType resourceSchemaTypeForDerived = resourceSchemaType.ChangeResourceType(derivedType);
            Type derivedSchemaType = resourceSchemaTypeForDerived.SchemaConstructedType;

            OpenApiSchema referenceSchemaForDerived = _defaultSchemaGenerator.GenerateSchema(derivedSchemaType, schemaRepository);
            OpenApiSchema fullSchemaForDerived = schemaRepository.Schemas[referenceSchemaForDerived.Reference.Id];
            fullSchemaForDerived.AdditionalPropertiesAllowed = false;

            OpenApiSchema inlineSchemaForDerived = fullSchemaForDerived.UnwrapLastExtendedSchema();
            SetResourceFields(inlineSchemaForDerived, resourceSchemaTypeForDerived, forRequestSchema, schemaRepository);

            SetAbstract(inlineSchemaForDerived, resourceSchemaTypeForDerived);
            RemoveProperties(inlineSchemaForDerived);
            MapInDiscriminator(resourceSchemaTypeForDerived, forRequestSchema, JsonApiPropertyName.Type, schemaRepository);

            if (fullSchemaForDerived.AllOf.Count == 0)
            {
                var compositeSchemaForDerived = new OpenApiSchema
                {
                    AllOf =
                    [
                        referenceSchemaForBase,
                        fullSchemaForDerived
                    ],
                    AdditionalPropertiesAllowed = false
                };

                schemaRepository.Schemas[referenceSchemaForDerived.Reference.Id] = compositeSchemaForDerived;
            }
            else
            {
                fullSchemaForDerived.AllOf[0] = referenceSchemaForBase;
            }

            if (RequiresRootObjectTypeInDataSchema(resourceSchemaTypeForDerived, forRequestSchema))
            {
                OpenApiSchema fullSchemaForData = schemaRepository.Schemas[referenceSchemaForDerived.Reference.Id];
                fullSchemaForData.Extensions[SetSchemaTypeToObjectDocumentFilter.RequiresRootObjectTypeKey] = new OpenApiBoolean(true);
            }

            GenerateDataSchemasForDirectlyDerivedTypes(resourceSchemaTypeForDerived, forRequestSchema, schemaRepository);
        }
    }

    private static void RemoveProperties(OpenApiSchema fullSchema)
    {
        foreach (string propertyName in fullSchema.Properties.Keys)
        {
            fullSchema.Properties.Remove(propertyName);
            fullSchema.Required.Remove(propertyName);
        }
    }

    private static bool RequiresRootObjectTypeInDataSchema(ResourceSchemaType resourceSchemaType, bool forRequestSchema)
    {
        Type? commonDataSchemaType = GetCommonSchemaType(resourceSchemaType.SchemaOpenType);

        if (forRequestSchema && (commonDataSchemaType == typeof(IdentifierInRequest) ||
            (!resourceSchemaType.ResourceType.ClrType.IsAbstract && commonDataSchemaType is { IsGenericType: false })))
        {
            // Bug workaround for NSwag, which fails to properly infer implicit { "type": "object" } of outer schema when it appears inside an allOf.
            // As a result, the required Data property in the generated client is assigned "default!" instead of a new instance.
            // But there's another bug on top of that: When the declared type of Data is abstract, it still generates assignment with a new instance, which fails to compile.
            return true;
        }

        return false;
    }
}
