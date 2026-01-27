using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;

internal sealed class RelationshipIdentifierSchemaGenerator
{
    private readonly SchemaGenerationTracer _schemaGenerationTracer;
    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly ResourceTypeSchemaGenerator _resourceTypeSchemaGenerator;
    private readonly ResourceIdSchemaGenerator _resourceIdSchemaGenerator;
    private readonly RelationshipNameSchemaGenerator _relationshipNameSchemaGenerator;
    private readonly JsonApiSchemaIdSelector _schemaIdSelector;

    public RelationshipIdentifierSchemaGenerator(SchemaGenerationTracer schemaGenerationTracer, SchemaGenerator defaultSchemaGenerator,
        ResourceTypeSchemaGenerator resourceTypeSchemaGenerator, ResourceIdSchemaGenerator resourceIdSchemaGenerator,
        RelationshipNameSchemaGenerator relationshipNameSchemaGenerator, JsonApiSchemaIdSelector schemaIdSelector)
    {
        ArgumentNullException.ThrowIfNull(schemaGenerationTracer);
        ArgumentNullException.ThrowIfNull(defaultSchemaGenerator);
        ArgumentNullException.ThrowIfNull(resourceTypeSchemaGenerator);
        ArgumentNullException.ThrowIfNull(resourceIdSchemaGenerator);
        ArgumentNullException.ThrowIfNull(relationshipNameSchemaGenerator);
        ArgumentNullException.ThrowIfNull(schemaIdSelector);

        _schemaGenerationTracer = schemaGenerationTracer;
        _defaultSchemaGenerator = defaultSchemaGenerator;
        _resourceTypeSchemaGenerator = resourceTypeSchemaGenerator;
        _resourceIdSchemaGenerator = resourceIdSchemaGenerator;
        _relationshipNameSchemaGenerator = relationshipNameSchemaGenerator;
        _schemaIdSelector = schemaIdSelector;
    }

    public OpenApiSchemaReference GenerateSchema(RelationshipAttribute relationship, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(relationship);
        ArgumentNullException.ThrowIfNull(schemaRepository);

        string schemaId = _schemaIdSelector.GetRelationshipIdentifierSchemaId(relationship);

        if (schemaRepository.Schemas.ContainsKey(schemaId))
        {
            return new OpenApiSchemaReference(schemaId);
        }

        using ISchemaGenerationTraceScope traceScope = _schemaGenerationTracer.TraceStart(this, relationship);

        Type relationshipIdentifierConstructedType = typeof(RelationshipIdentifier<>).MakeGenericType(relationship.LeftType.ClrType);
        ConsistencyGuard.ThrowIf(schemaRepository.TryLookupByTypeSafe(relationshipIdentifierConstructedType, out _));

        OpenApiSchemaReference referenceSchemaForIdentifier =
            _defaultSchemaGenerator.GenerateSchema(relationshipIdentifierConstructedType, schemaRepository).AsReferenceSchema();

        OpenApiSchema inlineSchemaForIdentifier = schemaRepository.Schemas[referenceSchemaForIdentifier.GetReferenceId()].AsInlineSchema();

        inlineSchemaForIdentifier.Properties ??= new Dictionary<string, IOpenApiSchema>();
        inlineSchemaForIdentifier.Properties.Remove(JsonApiPropertyName.Meta);

        SetResourceType(inlineSchemaForIdentifier.Properties, relationship.LeftType, schemaRepository);
        SetResourceId(inlineSchemaForIdentifier.Properties, relationship.LeftType, schemaRepository);
        SetRelationship(inlineSchemaForIdentifier.Properties, relationship, schemaRepository);

        schemaRepository.ReplaceSchemaId(relationshipIdentifierConstructedType, schemaId);
        referenceSchemaForIdentifier = new OpenApiSchemaReference(schemaId);

        traceScope.TraceSucceeded(schemaId);
        return referenceSchemaForIdentifier;
    }

    private void SetResourceType(IDictionary<string, IOpenApiSchema> schemaProperties, ResourceType resourceType, SchemaRepository schemaRepository)
    {
        OpenApiSchemaReference referenceSchema = _resourceTypeSchemaGenerator.GenerateSchema(resourceType, schemaRepository);
        schemaProperties[JsonApiPropertyName.Type] = referenceSchema.WrapInExtendedSchema();
    }

    private void SetResourceId(IDictionary<string, IOpenApiSchema> schemaProperties, ResourceType resourceType, SchemaRepository schemaRepository)
    {
        OpenApiSchema idSchema = _resourceIdSchemaGenerator.GenerateSchema(resourceType, schemaRepository);
        schemaProperties[JsonApiPropertyName.Id] = idSchema;
    }

    private void SetRelationship(IDictionary<string, IOpenApiSchema> schemaProperties, RelationshipAttribute relationship, SchemaRepository schemaRepository)
    {
        OpenApiSchemaReference referenceSchema = _relationshipNameSchemaGenerator.GenerateSchema(relationship, schemaRepository);
        schemaProperties[JsonApiPropertyName.Relationship] = referenceSchema.WrapInExtendedSchema();
    }
}
