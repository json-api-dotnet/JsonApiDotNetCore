using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;
using JsonApiDotNetCore.Resources;
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

        Type? commonDataSchemaType = GetCommonDataSchemaType(resourceSchemaType.SchemaOpenType);

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
            MapDataInDiscriminator(resourceSchemaType, forRequestSchema, schemaRepository);
        }

        if (RequiresRootObjectTypeInDataSchema(resourceSchemaType, forRequestSchema))
        {
            fullSchemaForData.Extensions[SetSchemaTypeToObjectDocumentFilter.RequiresRootObjectTypeKey] = new OpenApiBoolean(true);
        }

        return referenceSchemaForData;
    }

    private static Type? GetCommonDataSchemaType(Type schemaOpenType)
    {
        if (schemaOpenType == typeof(ResourceIdentifierInRequest<>))
        {
            return typeof(ResourceIdentifierInRequest);
        }

        if (schemaOpenType == typeof(DataInCreateResourceRequest<>))
        {
            return typeof(DataInCreateResourceRequest);
        }

        if (schemaOpenType == typeof(DataInUpdateResourceRequest<>))
        {
            return typeof(DataInUpdateResourceRequest);
        }

        if (schemaOpenType == typeof(ResourceDataInResponse<>))
        {
            return typeof(ResourceDataInResponse);
        }

        return null;
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
            Required = new SortedSet<string>([JsonApiPropertyName.Type]),
            Type = "object",
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
        if (resourceSchemaType.ResourceType.ClrType.IsAbstract && resourceSchemaType.SchemaOpenType != typeof(ResourceIdentifierInRequest<>))
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

        if (resourceSchemaType.SchemaOpenType == typeof(DataInCreateResourceRequest<>))
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

    private static void SetFieldSchemaMembers(OpenApiSchema fullSchemaForData, ResourceSchemaType resourceSchemaType, bool forRequestSchema, bool forAttributes,
        ResourceFieldSchemaBuilder fieldSchemaBuilder, SchemaRepository schemaRepository)
    {
        string propertyNameInSchema = forAttributes ? JsonApiPropertyName.Attributes : JsonApiPropertyName.Relationships;

        OpenApiSchema referenceSchemaForFields = fullSchemaForData.Properties[propertyNameInSchema].UnwrapLastExtendedSchema();
        OpenApiSchema fullSchemaForFields = schemaRepository.Schemas[referenceSchemaForFields.Reference.Id];
        fullSchemaForFields.AdditionalPropertiesAllowed = false;

        SetAbstract(fullSchemaForFields, resourceSchemaType);

        if (forAttributes)
        {
            fieldSchemaBuilder.SetMembersOfAttributes(fullSchemaForFields, forRequestSchema, schemaRepository);
        }
        else
        {
            fieldSchemaBuilder.SetMembersOfRelationships(fullSchemaForFields, forRequestSchema, schemaRepository);
        }

        if (fullSchemaForFields.Properties.Count == 0 && !resourceSchemaType.ResourceType.IsPartOfTypeHierarchy())
        {
            fullSchemaForData.Properties.Remove(propertyNameInSchema);
            schemaRepository.Schemas.Remove(referenceSchemaForFields.Reference.Id);
        }

        if (resourceSchemaType.ResourceType.IsPartOfTypeHierarchy())
        {
            if (resourceSchemaType.ResourceType.BaseType == null)
            {
                CreateFieldsDiscriminator(fullSchemaForFields);
            }
            else
            {
                string propertyNameInSchemaType = forAttributes
                    ? nameof(ResourceDataInResponse<IIdentifiable>.Attributes)
                    : nameof(ResourceDataInResponse<IIdentifiable>.Relationships);

                Type fieldsSchemaType = GetSchemaTypeForProperty(resourceSchemaType.SchemaConstructedType, propertyNameInSchemaType);
                MapFieldsInDiscriminator(fieldsSchemaType, resourceSchemaType.ResourceType, schemaRepository);

                Type baseSchemaType = ChangeResourceTypeInSchemaType(fieldsSchemaType, resourceSchemaType.ResourceType.BaseType);
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
    }

    private static void CreateFieldsDiscriminator(OpenApiSchema fullSchema)
    {
        fullSchema.Properties.Add(OpenApiMediaTypeExtension.FullyQualifiedOpenApiDiscriminatorPropertyName, new OpenApiSchema
        {
            Type = "string"
        });

        fullSchema.Required.Add(OpenApiMediaTypeExtension.FullyQualifiedOpenApiDiscriminatorPropertyName);

        fullSchema.Discriminator = new OpenApiDiscriminator
        {
            PropertyName = OpenApiMediaTypeExtension.FullyQualifiedOpenApiDiscriminatorPropertyName,
            Mapping = new SortedDictionary<string, string>(StringComparer.Ordinal)
        };
    }

    private static Type GetSchemaTypeForProperty(Type containingType, string propertyName)
    {
        PropertyInfo? propertyInfo = containingType.GetProperty(propertyName);

        if (propertyInfo == null)
        {
            throw new UnreachableException();
        }

        return propertyInfo.PropertyType;
    }

    private static void MapFieldsInDiscriminator(Type schemaType, ResourceType resourceType, SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchemaForDerived = schemaRepository.LookupByType(schemaType);
        ResourceType ultimateBaseResourceType = GetUltimateBaseType(resourceType);
        Type ultimateBaseSchemaType = ChangeResourceTypeInSchemaType(schemaType, ultimateBaseResourceType);

        OpenApiSchema referenceSchemaForUltimateBase = schemaRepository.LookupByType(ultimateBaseSchemaType);
        OpenApiSchema fullSchemaForUltimateBase = schemaRepository.Schemas[referenceSchemaForUltimateBase.Reference.Id];
        fullSchemaForUltimateBase.Discriminator.Mapping.Add(resourceType.PublicName, referenceSchemaForDerived.Reference.ReferenceV3);
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
            MapDataInDiscriminator(resourceSchemaTypeForDerived, forRequestSchema, schemaRepository);

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

    private void MapDataInDiscriminator(ResourceSchemaType resourceSchemaType, bool forRequestSchema, SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchemaForDerived = schemaRepository.LookupByType(resourceSchemaType.SchemaConstructedType);

        foreach (ResourceType? baseResourceType in GetBaseResourceTypesToMapInto(resourceSchemaType, forRequestSchema))
        {
            Type baseSchemaType = baseResourceType == null
                ? GetCommonDataSchemaType(resourceSchemaType.SchemaOpenType)!
                : resourceSchemaType.ChangeResourceType(baseResourceType).SchemaConstructedType;

            OpenApiSchema referenceSchemaForBase = schemaRepository.LookupByType(baseSchemaType);
            OpenApiSchema inlineSchemaForBase = schemaRepository.Schemas[referenceSchemaForBase.Reference.Id].UnwrapLastExtendedSchema();

            inlineSchemaForBase.Discriminator ??= new OpenApiDiscriminator
            {
                PropertyName = JsonApiPropertyName.Type,
                Mapping = new SortedDictionary<string, string>(StringComparer.Ordinal)
            };

            string publicName = resourceSchemaType.ResourceType.PublicName;

            if (inlineSchemaForBase.Discriminator.Mapping.TryAdd(publicName, referenceSchemaForDerived.Reference.ReferenceV3) && baseResourceType == null)
            {
                MapResourceTypeInEnum(publicName, schemaRepository);
            }
        }
    }

    private static IEnumerable<ResourceType?> GetBaseResourceTypesToMapInto(ResourceSchemaType resourceSchemaType, bool forRequestSchema)
    {
        bool dependsOnCommonDataSchemaType = GetCommonDataSchemaType(resourceSchemaType.SchemaOpenType) != null;

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
            if (!dependsOnCommonDataSchemaType)
            {
                yield return GetUltimateBaseType(resourceSchemaType.ResourceType);
            }
        }

        if (dependsOnCommonDataSchemaType)
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

    private static bool RequiresRootObjectTypeInDataSchema(ResourceSchemaType resourceSchemaType, bool forRequestSchema)
    {
        Type? commonDataSchemaType = GetCommonDataSchemaType(resourceSchemaType.SchemaOpenType);

        if (forRequestSchema && (commonDataSchemaType == typeof(ResourceIdentifierInRequest) ||
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
