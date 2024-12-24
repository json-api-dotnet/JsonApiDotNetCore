using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;

internal sealed class RelationshipIdentifierSchemaGenerator
{
    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly ResourceTypeSchemaGenerator _resourceTypeSchemaGenerator;
    private readonly ResourceIdSchemaGenerator _resourceIdSchemaGenerator;
    private readonly RelationshipNameSchemaGenerator _relationshipNameSchemaGenerator;
    private readonly JsonApiSchemaIdSelector _schemaIdSelector;

    public RelationshipIdentifierSchemaGenerator(SchemaGenerator defaultSchemaGenerator, ResourceTypeSchemaGenerator resourceTypeSchemaGenerator,
        ResourceIdSchemaGenerator resourceIdSchemaGenerator, RelationshipNameSchemaGenerator relationshipNameSchemaGenerator,
        JsonApiSchemaIdSelector schemaIdSelector)
    {
        ArgumentNullException.ThrowIfNull(defaultSchemaGenerator);
        ArgumentNullException.ThrowIfNull(resourceTypeSchemaGenerator);
        ArgumentNullException.ThrowIfNull(resourceIdSchemaGenerator);
        ArgumentNullException.ThrowIfNull(relationshipNameSchemaGenerator);
        ArgumentNullException.ThrowIfNull(schemaIdSelector);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _resourceTypeSchemaGenerator = resourceTypeSchemaGenerator;
        _resourceIdSchemaGenerator = resourceIdSchemaGenerator;
        _relationshipNameSchemaGenerator = relationshipNameSchemaGenerator;
        _schemaIdSelector = schemaIdSelector;
    }

    public OpenApiSchema GenerateSchema(RelationshipAttribute relationship, SchemaRepository schemaRepository)
    {
        ArgumentNullException.ThrowIfNull(relationship);
        ArgumentNullException.ThrowIfNull(schemaRepository);

        string schemaId = _schemaIdSelector.GetRelationshipIdentifierSchemaId(relationship);

        if (schemaRepository.Schemas.ContainsKey(schemaId))
        {
            return new OpenApiSchema
            {
                Reference = new OpenApiReference
                {
                    Id = schemaId,
                    Type = ReferenceType.Schema
                }
            };
        }

        Type relationshipIdentifierConstructedType = typeof(RelationshipIdentifier<>).MakeGenericType(relationship.LeftType.ClrType);

        if (schemaRepository.TryLookupByType(relationshipIdentifierConstructedType, out _))
        {
            throw new UnreachableCodeException();
        }

        OpenApiSchema referenceSchemaForIdentifier = _defaultSchemaGenerator.GenerateSchema(relationshipIdentifierConstructedType, schemaRepository);
        OpenApiSchema fullSchemaForIdentifier = schemaRepository.Schemas[referenceSchemaForIdentifier.Reference.Id];

        fullSchemaForIdentifier.Properties.Remove(JsonApiPropertyName.Meta);

        SetResourceType(fullSchemaForIdentifier, relationship.LeftType, schemaRepository);
        SetResourceId(fullSchemaForIdentifier, relationship.LeftType, schemaRepository);
        SetRelationship(fullSchemaForIdentifier, relationship, schemaRepository);

        schemaRepository.ReplaceSchemaId(relationshipIdentifierConstructedType, schemaId);
        referenceSchemaForIdentifier.Reference.Id = schemaId;

        return referenceSchemaForIdentifier;
    }

    private void SetResourceType(OpenApiSchema fullSchemaForIdentifier, ResourceType resourceType, SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchema = _resourceTypeSchemaGenerator.GenerateSchema(resourceType, schemaRepository);
        fullSchemaForIdentifier.Properties[JsonApiPropertyName.Type] = referenceSchema.WrapInExtendedSchema();
    }

    private void SetResourceId(OpenApiSchema fullSchemaForResourceData, ResourceType resourceType, SchemaRepository schemaRepository)
    {
        OpenApiSchema idSchema = _resourceIdSchemaGenerator.GenerateSchema(resourceType, schemaRepository);
        fullSchemaForResourceData.Properties[JsonApiPropertyName.Id] = idSchema;
    }

    private void SetRelationship(OpenApiSchema fullSchemaForIdentifier, RelationshipAttribute relationship, SchemaRepository schemaRepository)
    {
        OpenApiSchema referenceSchema = _relationshipNameSchemaGenerator.GenerateSchema(relationship, schemaRepository);
        fullSchemaForIdentifier.Properties[JsonApiPropertyName.Relationship] = referenceSchema.WrapInExtendedSchema();
    }
}
