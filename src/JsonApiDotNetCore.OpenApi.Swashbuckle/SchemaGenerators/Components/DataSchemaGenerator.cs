using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;
using Microsoft.OpenApi;
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

    private readonly SchemaGenerationTracer _schemaGenerationTracer;
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

    public DataSchemaGenerator(SchemaGenerationTracer schemaGenerationTracer, SchemaGenerator defaultSchemaGenerator,
        GenerationCacheSchemaGenerator generationCacheSchemaGenerator, ResourceTypeSchemaGenerator resourceTypeSchemaGenerator,
        ResourceIdSchemaGenerator resourceIdSchemaGenerator, LinksVisibilitySchemaGenerator linksVisibilitySchemaGenerator,
        MetaSchemaGenerator metaSchemaGenerator, JsonApiSchemaIdSelector schemaIdSelector, IJsonApiOptions options, IResourceGraph resourceGraph,
        ResourceFieldValidationMetadataProvider resourceFieldValidationMetadataProvider, RelationshipTypeFactory relationshipTypeFactory,
        ResourceDocumentationReader resourceDocumentationReader)
    {
        ArgumentNullException.ThrowIfNull(schemaGenerationTracer);
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

        _schemaGenerationTracer = schemaGenerationTracer;
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

    public OpenApiSchemaReference GenerateSchema(Type dataSchemaType, bool forRequestSchema, SchemaRepository schemaRepository)
    {
        // For a given resource (identifier) type, we always generate the full type hierarchy. Discriminator mappings
        // are managed manually, because there's no way to intercept in the Swashbuckle recursive component schema generation.

        ArgumentNullException.ThrowIfNull(dataSchemaType);
        ArgumentNullException.ThrowIfNull(schemaRepository);

        if (schemaRepository.TryLookupByTypeSafe(dataSchemaType, out OpenApiSchemaReference? referenceSchemaForData))
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

        using ISchemaGenerationTraceScope traceScope = _schemaGenerationTracer.TraceStart(this, dataSchemaType);

        referenceSchemaForData = _defaultSchemaGenerator.GenerateSchema(dataSchemaType, schemaRepository).AsReferenceSchema();
        OpenApiSchema possiblyCompositeInlineSchemaForData = schemaRepository.Schemas[referenceSchemaForData.GetReferenceId()].AsInlineSchema();
        possiblyCompositeInlineSchemaForData.AdditionalPropertiesAllowed = false;

        OpenApiSchema inlineSchemaForData = possiblyCompositeInlineSchemaForData.UnwrapLastExtendedSchema().AsInlineSchema();

        SetAbstract(inlineSchemaForData, resourceSchemaType);
        SetResourceType(inlineSchemaForData, resourceType, schemaRepository);
        AdaptResourceIdentity(inlineSchemaForData, resourceSchemaType, forRequestSchema, schemaRepository);
        SetResourceId(inlineSchemaForData, resourceType, schemaRepository);
        SetResourceFields(inlineSchemaForData, resourceSchemaType, forRequestSchema, schemaRepository);
        SetDocumentation(possiblyCompositeInlineSchemaForData, resourceType);
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
            possiblyCompositeInlineSchemaForData.Extensions ??= new SortedDictionary<string, IOpenApiExtension>(StringComparer.Ordinal);
            possiblyCompositeInlineSchemaForData.Extensions[SetSchemaTypeToObjectDocumentFilter.RequiresRootObjectTypeKey] = new JsonNodeExtension(true);
        }

        traceScope.TraceSucceeded(referenceSchemaForData.GetReferenceId());
        return referenceSchemaForData;
    }

    private static Type? GetCommonSchemaType(Type schemaOpenType)
    {
        StrongBox<Type?>? boxedSchemaType = null;

        if (schemaOpenType == typeof(IdentifierInRequest<>))
        {
            boxedSchemaType = new StrongBox<Type?>(typeof(IdentifierInRequest));
        }
        else if (schemaOpenType == typeof(DataInCreateRequest<>))
        {
            boxedSchemaType = new StrongBox<Type?>(typeof(ResourceInCreateRequest));
        }
        else if (schemaOpenType == typeof(AttributesInCreateRequest<>))
        {
            boxedSchemaType = new StrongBox<Type?>(typeof(AttributesInCreateRequest));
        }
        else if (schemaOpenType == typeof(RelationshipsInCreateRequest<>))
        {
            boxedSchemaType = new StrongBox<Type?>(typeof(RelationshipsInCreateRequest));
        }
        else if (schemaOpenType == typeof(DataInUpdateRequest<>))
        {
            boxedSchemaType = new StrongBox<Type?>(typeof(ResourceInUpdateRequest));
        }
        else if (schemaOpenType == typeof(AttributesInUpdateRequest<>))
        {
            boxedSchemaType = new StrongBox<Type?>(typeof(AttributesInUpdateRequest));
        }
        else if (schemaOpenType == typeof(RelationshipsInUpdateRequest<>))
        {
            boxedSchemaType = new StrongBox<Type?>(typeof(RelationshipsInUpdateRequest));
        }
        else if (schemaOpenType == typeof(IdentifierInResponse<>))
        {
            boxedSchemaType = new StrongBox<Type?>(null);
        }
        else if (schemaOpenType == typeof(DataInResponse<>))
        {
            boxedSchemaType = new StrongBox<Type?>(typeof(ResourceInResponse));
        }
        else if (schemaOpenType == typeof(AttributesInResponse<>))
        {
            boxedSchemaType = new StrongBox<Type?>(typeof(AttributesInResponse));
        }
        else if (schemaOpenType == typeof(RelationshipsInResponse<>))
        {
            boxedSchemaType = new StrongBox<Type?>(typeof(RelationshipsInResponse));
        }

        ConsistencyGuard.ThrowIf(boxedSchemaType == null);
        return boxedSchemaType.Value;
    }

    public OpenApiSchemaReference GenerateSchemaForCommonData(Type commonDataSchemaType, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(commonDataSchemaType);
        ArgumentNullException.ThrowIfNull(schemaRepository);

        if (schemaRepository.TryLookupByTypeSafe(commonDataSchemaType, out OpenApiSchemaReference? referenceSchema))
        {
            return referenceSchema;
        }

        using ISchemaGenerationTraceScope traceScope = _schemaGenerationTracer.TraceStart(this, commonDataSchemaType);

        OpenApiSchemaReference referenceSchemaForResourceType = _resourceTypeSchemaGenerator.GenerateSchema(schemaRepository);
        OpenApiSchemaReference referenceSchemaForMeta = _metaSchemaGenerator.GenerateSchema(schemaRepository);

        var inlineSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Required = new SortedSet<string>([JsonApiPropertyName.Type], StringComparer.Ordinal),
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                [JsonApiPropertyName.Type] = referenceSchemaForResourceType.WrapInExtendedSchema(),
                [referenceSchemaForMeta.GetReferenceId()] = referenceSchemaForMeta.WrapInExtendedSchema()
            },
            AdditionalPropertiesAllowed = false,
            Discriminator = new OpenApiDiscriminator
            {
                PropertyName = JsonApiPropertyName.Type
            },
            Extensions = new SortedDictionary<string, IOpenApiExtension>(StringComparer.Ordinal)
            {
                ["x-abstract"] = new JsonNodeExtension(true)
            }
        };

        string schemaId = _schemaIdSelector.GetSchemaId(commonDataSchemaType);

        referenceSchema = schemaRepository.AddDefinition(schemaId, inlineSchema);
        schemaRepository.RegisterType(commonDataSchemaType, schemaId);

        traceScope.TraceSucceeded(schemaId);
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

    private static void SetAbstract(OpenApiSchema inlineSchema, ResourceSchemaType resourceSchemaType)
    {
        if (resourceSchemaType.ResourceType.ClrType.IsAbstract && resourceSchemaType.SchemaOpenType != typeof(IdentifierInRequest<>))
        {
            inlineSchema.Extensions ??= new SortedDictionary<string, IOpenApiExtension>(StringComparer.Ordinal);
            inlineSchema.Extensions["x-abstract"] = new JsonNodeExtension(true);
        }
    }

    private void SetResourceType(OpenApiSchema inlineSchema, ResourceType resourceType, SchemaRepository schemaRepository)
    {
        if (inlineSchema.Properties != null && inlineSchema.Properties.ContainsKey(JsonApiPropertyName.Type))
        {
            OpenApiSchemaReference referenceSchema = _resourceTypeSchemaGenerator.GenerateSchema(resourceType, schemaRepository);
            inlineSchema.Properties[JsonApiPropertyName.Type] = referenceSchema.WrapInExtendedSchema();
        }
    }

    private void AdaptResourceIdentity(OpenApiSchema inlineSchema, ResourceSchemaType resourceSchemaType, bool forRequestSchema,
        SchemaRepository schemaRepository)
    {
        if (!forRequestSchema)
        {
            return;
        }

        bool hasAtomicOperationsEndpoint = _generationCacheSchemaGenerator.HasAtomicOperationsEndpoint(schemaRepository);

        if (!hasAtomicOperationsEndpoint)
        {
            inlineSchema.Properties?.Remove(JsonApiPropertyName.Lid);
        }

        if (resourceSchemaType.SchemaOpenType == typeof(DataInCreateRequest<>))
        {
            ClientIdGenerationMode clientIdGeneration = resourceSchemaType.ResourceType.ClientIdGeneration ?? _options.ClientIdGeneration;

            if (hasAtomicOperationsEndpoint)
            {
                if (clientIdGeneration == ClientIdGenerationMode.Forbidden)
                {
                    inlineSchema.Properties?.Remove(JsonApiPropertyName.Id);
                }
                else if (clientIdGeneration == ClientIdGenerationMode.Required)
                {
                    inlineSchema.Properties?.Remove(JsonApiPropertyName.Lid);
                    inlineSchema.Required ??= new SortedSet<string>(StringComparer.Ordinal);
                    inlineSchema.Required.Add(JsonApiPropertyName.Id);
                }
            }
            else
            {
                if (clientIdGeneration == ClientIdGenerationMode.Forbidden)
                {
                    inlineSchema.Properties?.Remove(JsonApiPropertyName.Id);
                }
                else if (clientIdGeneration == ClientIdGenerationMode.Required)
                {
                    inlineSchema.Required ??= new SortedSet<string>(StringComparer.Ordinal);
                    inlineSchema.Required.Add(JsonApiPropertyName.Id);
                }
            }
        }
        else
        {
            if (!hasAtomicOperationsEndpoint)
            {
                inlineSchema.Required ??= new SortedSet<string>(StringComparer.Ordinal);
                inlineSchema.Required.Add(JsonApiPropertyName.Id);
            }
        }
    }

    private void SetResourceId(OpenApiSchema inlineSchema, ResourceType resourceType, SchemaRepository schemaRepository)
    {
        if (inlineSchema.Properties != null && inlineSchema.Properties.ContainsKey(JsonApiPropertyName.Id))
        {
            OpenApiSchema idSchema = _resourceIdSchemaGenerator.GenerateSchema(resourceType, schemaRepository);
            inlineSchema.Properties[JsonApiPropertyName.Id] = idSchema;
        }
    }

    private void SetResourceFields(OpenApiSchema inlineSchema, ResourceSchemaType resourceSchemaType, bool forRequestSchema, SchemaRepository schemaRepository)
    {
        if (inlineSchema.Properties != null && inlineSchema.Properties.ContainsKey(JsonApiPropertyName.Attributes) &&
            inlineSchema.Properties.ContainsKey(JsonApiPropertyName.Relationships))
        {
            var fieldSchemaBuilder = new ResourceFieldSchemaBuilder(_schemaGenerationTracer, _defaultSchemaGenerator, this, _linksVisibilitySchemaGenerator,
                _resourceFieldValidationMetadataProvider, _relationshipTypeFactory, resourceSchemaType);

            SetFieldSchemaMembers(inlineSchema.Properties, resourceSchemaType, forRequestSchema, true, fieldSchemaBuilder, schemaRepository);
            SetFieldSchemaMembers(inlineSchema.Properties, resourceSchemaType, forRequestSchema, false, fieldSchemaBuilder, schemaRepository);
        }
    }

    private void SetFieldSchemaMembers(IDictionary<string, IOpenApiSchema> inlineSchemaForDataProperties, ResourceSchemaType resourceSchemaTypeForData,
        bool forRequestSchema, bool forAttributes, ResourceFieldSchemaBuilder fieldSchemaBuilder, SchemaRepository schemaRepository)
    {
        string propertyNameInSchema = forAttributes ? JsonApiPropertyName.Attributes : JsonApiPropertyName.Relationships;

        OpenApiSchemaReference referenceSchemaForFields = inlineSchemaForDataProperties[propertyNameInSchema].UnwrapLastExtendedSchema().AsReferenceSchema();
        OpenApiSchema inlineSchemaForFields = schemaRepository.Schemas[referenceSchemaForFields.GetReferenceId()].AsInlineSchema();
        inlineSchemaForFields.AdditionalPropertiesAllowed = false;

        SetAbstract(inlineSchemaForFields, resourceSchemaTypeForData);

        if (forAttributes)
        {
            fieldSchemaBuilder.SetMembersOfAttributes(inlineSchemaForFields, forRequestSchema, schemaRepository);
        }
        else
        {
            fieldSchemaBuilder.SetMembersOfRelationships(inlineSchemaForFields, forRequestSchema, schemaRepository);
        }

        if ((inlineSchemaForFields.Properties == null || inlineSchemaForFields.Properties.Count == 0) &&
            !resourceSchemaTypeForData.ResourceType.IsPartOfTypeHierarchy())
        {
            inlineSchemaForDataProperties.Remove(propertyNameInSchema);
            schemaRepository.Schemas.Remove(referenceSchemaForFields.GetReferenceId());
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

            OpenApiSchemaReference referenceSchemaForBase = schemaRepository.LookupByType(baseSchemaType);

            schemaRepository.Schemas[referenceSchemaForFields.GetReferenceId()] = new OpenApiSchema
            {
                AllOf =
                [
                    referenceSchemaForBase,
                    inlineSchemaForFields
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

    private OpenApiSchemaReference GenerateSchemaForCommonFields(Type commonFieldsSchemaType, SchemaRepository schemaRepository)
    {
        if (schemaRepository.TryLookupByTypeSafe(commonFieldsSchemaType, out OpenApiSchemaReference? referenceSchema))
        {
            return referenceSchema;
        }

        using ISchemaGenerationTraceScope traceScope = _schemaGenerationTracer.TraceStart(this, commonFieldsSchemaType);

        OpenApiSchemaReference referenceSchemaForResourceType = _resourceTypeSchemaGenerator.GenerateSchema(schemaRepository);

        var inlineSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Required = new SortedSet<string>([OpenApiMediaTypeExtension.FullyQualifiedOpenApiDiscriminatorPropertyName], StringComparer.Ordinal),
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                [OpenApiMediaTypeExtension.FullyQualifiedOpenApiDiscriminatorPropertyName] = referenceSchemaForResourceType.WrapInExtendedSchema()
            },
            AdditionalPropertiesAllowed = false,
            Discriminator = new OpenApiDiscriminator
            {
                PropertyName = OpenApiMediaTypeExtension.FullyQualifiedOpenApiDiscriminatorPropertyName
            },
            Extensions = new SortedDictionary<string, IOpenApiExtension>(StringComparer.Ordinal)
            {
                ["x-abstract"] = new JsonNodeExtension(true)
            }
        };

        string schemaId = _schemaIdSelector.GetSchemaId(commonFieldsSchemaType);

        referenceSchema = schemaRepository.AddDefinition(schemaId, inlineSchema);
        schemaRepository.RegisterType(commonFieldsSchemaType, schemaId);

        traceScope.TraceSucceeded(schemaId);
        return referenceSchema;
    }

    private void MapInDiscriminator(ResourceSchemaType resourceSchemaType, bool forRequestSchema, string discriminatorPropertyName,
        SchemaRepository schemaRepository)
    {
        OpenApiSchemaReference referenceSchemaForDerived = schemaRepository.LookupByType(resourceSchemaType.SchemaConstructedType);

        foreach (ResourceType? baseResourceType in GetBaseTypesToMapInto(resourceSchemaType, forRequestSchema))
        {
            Type baseSchemaType = baseResourceType == null
                ? GetCommonSchemaType(resourceSchemaType.SchemaOpenType)!
                : resourceSchemaType.ChangeResourceType(baseResourceType).SchemaConstructedType;

            OpenApiSchemaReference referenceSchemaForBase = schemaRepository.LookupByType(baseSchemaType);
            OpenApiSchema inlineSchemaForBase = schemaRepository.Schemas[referenceSchemaForBase.GetReferenceId()].UnwrapLastExtendedSchema().AsInlineSchema();

            inlineSchemaForBase.Discriminator ??= new OpenApiDiscriminator
            {
                PropertyName = discriminatorPropertyName
            };

            if (RepeatDiscriminatorInResponseDerivedTypes && !forRequestSchema)
            {
                inlineSchemaForBase.Required ??= new SortedSet<string>(StringComparer.Ordinal);
                inlineSchemaForBase.Required.Add(discriminatorPropertyName);
            }

            string publicName = resourceSchemaType.ResourceType.PublicName;

            if (TryAddToDiscriminatorMapping(inlineSchemaForBase, publicName, referenceSchemaForDerived) && baseResourceType == null)
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

    private static bool TryAddToDiscriminatorMapping(OpenApiSchema inlineSchema, string schemaId, OpenApiSchemaReference mappingValueReferenceSchema)
    {
        ConsistencyGuard.ThrowIf(inlineSchema.Discriminator is null);

        inlineSchema.Discriminator.Mapping ??= new SortedDictionary<string, OpenApiSchemaReference>(StringComparer.Ordinal);
        return inlineSchema.Discriminator.Mapping.TryAdd(schemaId, mappingValueReferenceSchema);
    }

    private void MapResourceTypeInEnum(string publicName, SchemaRepository schemaRepository)
    {
        string schemaId = _schemaIdSelector.GetResourceTypeSchemaId(null);
        OpenApiSchema inlineSchema = schemaRepository.Schemas[schemaId].AsInlineSchema();
        inlineSchema.Enum ??= new List<JsonNode>();

        if (!inlineSchema.Enum.Any(jsonNode => jsonNode is JsonValue jsonValue && jsonValue.GetValueKind() == JsonValueKind.String &&
            jsonValue.GetValue<string>() == publicName))
        {
            inlineSchema.Enum.Add(publicName);
        }
    }

    private void SetDocumentation(OpenApiSchema inlineSchema, ResourceType resourceType)
    {
        inlineSchema.Description = _resourceDocumentationReader.GetDocumentationForType(resourceType);
    }

    private void SetLinksVisibility(OpenApiSchema inlineSchema, ResourceSchemaType resourceSchemaType, SchemaRepository schemaRepository)
    {
        _linksVisibilitySchemaGenerator.UpdateSchemaForResource(resourceSchemaType, inlineSchema, schemaRepository);
    }

    private void GenerateDataSchemasForDirectlyDerivedTypes(ResourceSchemaType resourceSchemaType, bool forRequestSchema, SchemaRepository schemaRepository)
    {
        OpenApiSchemaReference referenceSchemaForBase = schemaRepository.LookupByType(resourceSchemaType.SchemaConstructedType);

        foreach (ResourceType derivedType in resourceSchemaType.ResourceType.DirectlyDerivedTypes)
        {
            ResourceSchemaType resourceSchemaTypeForDerived = resourceSchemaType.ChangeResourceType(derivedType);
            Type derivedSchemaType = resourceSchemaTypeForDerived.SchemaConstructedType;

            using ISchemaGenerationTraceScope traceScope = _schemaGenerationTracer.TraceStart(this, resourceSchemaTypeForDerived.SchemaConstructedType);

            OpenApiSchemaReference referenceSchemaForDerived = _defaultSchemaGenerator.GenerateSchema(derivedSchemaType, schemaRepository).AsReferenceSchema();
            OpenApiSchema possiblyCompositeInlineSchemaForDerived = schemaRepository.Schemas[referenceSchemaForDerived.GetReferenceId()].AsInlineSchema();
            possiblyCompositeInlineSchemaForDerived.AdditionalPropertiesAllowed = false;

            OpenApiSchema inlineSchemaForDerived = possiblyCompositeInlineSchemaForDerived.UnwrapLastExtendedSchema().AsInlineSchema();
            SetResourceFields(inlineSchemaForDerived, resourceSchemaTypeForDerived, forRequestSchema, schemaRepository);

            SetAbstract(inlineSchemaForDerived, resourceSchemaTypeForDerived);
            RemoveProperties(inlineSchemaForDerived);
            MapInDiscriminator(resourceSchemaTypeForDerived, forRequestSchema, JsonApiPropertyName.Type, schemaRepository);

            if (possiblyCompositeInlineSchemaForDerived.AllOf == null || possiblyCompositeInlineSchemaForDerived.AllOf.Count == 0)
            {
                var compositeInlineSchemaForDerived = new OpenApiSchema
                {
                    AllOf =
                    [
                        referenceSchemaForBase,
                        possiblyCompositeInlineSchemaForDerived
                    ],
                    AdditionalPropertiesAllowed = false
                };

                schemaRepository.Schemas[referenceSchemaForDerived.GetReferenceId()] = compositeInlineSchemaForDerived;
            }
            else
            {
                possiblyCompositeInlineSchemaForDerived.AllOf[0] = referenceSchemaForBase;
            }

            if (RequiresRootObjectTypeInDataSchema(resourceSchemaTypeForDerived, forRequestSchema))
            {
                OpenApiSchema inlineSchemaForData = schemaRepository.Schemas[referenceSchemaForDerived.GetReferenceId()].AsInlineSchema();
                inlineSchemaForData.Extensions ??= new SortedDictionary<string, IOpenApiExtension>(StringComparer.Ordinal);
                inlineSchemaForData.Extensions[SetSchemaTypeToObjectDocumentFilter.RequiresRootObjectTypeKey] = new JsonNodeExtension(true);
            }

            GenerateDataSchemasForDirectlyDerivedTypes(resourceSchemaTypeForDerived, forRequestSchema, schemaRepository);

            traceScope.TraceSucceeded(referenceSchemaForDerived.GetReferenceId());
        }
    }

    private static void RemoveProperties(OpenApiSchema inlineSchema)
    {
        if (inlineSchema.Properties != null)
        {
            foreach (string propertyName in inlineSchema.Properties.Keys)
            {
                inlineSchema.Properties.Remove(propertyName);
                inlineSchema.Required?.Remove(propertyName);
            }
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
