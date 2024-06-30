using JsonApiDotNetCore.OpenApi.JsonApiObjects.ResourceObjects;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SchemaGenerators.Components;

internal sealed class RelationshipIdentifierSchemaGenerator
{
#if NET6_0
    private static readonly string[] RelationshipIdentifierPropertyNamesInOrder =
    [
        JsonApiPropertyName.Type,
        JsonApiPropertyName.Id,
        JsonApiPropertyName.Lid,
        JsonApiPropertyName.Relationship
    ];
#endif

    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly ResourceTypeSchemaGenerator _resourceTypeSchemaGenerator;
    private readonly RelationshipNameSchemaGenerator _relationshipNameSchemaGenerator;
    private readonly JsonApiSchemaIdSelector _jsonApiSchemaIdSelector;

    public RelationshipIdentifierSchemaGenerator(SchemaGenerator defaultSchemaGenerator, ResourceTypeSchemaGenerator resourceTypeSchemaGenerator,
        RelationshipNameSchemaGenerator relationshipNameSchemaGenerator, JsonApiSchemaIdSelector jsonApiSchemaIdSelector)
    {
        ArgumentGuard.NotNull(defaultSchemaGenerator);
        ArgumentGuard.NotNull(resourceTypeSchemaGenerator);
        ArgumentGuard.NotNull(relationshipNameSchemaGenerator);
        ArgumentGuard.NotNull(jsonApiSchemaIdSelector);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _resourceTypeSchemaGenerator = resourceTypeSchemaGenerator;
        _relationshipNameSchemaGenerator = relationshipNameSchemaGenerator;
        _jsonApiSchemaIdSelector = jsonApiSchemaIdSelector;
    }

    public OpenApiSchema GenerateSchema(RelationshipAttribute relationship, SchemaRepository schemaRepository)
    {
        ArgumentGuard.NotNull(relationship);
        ArgumentGuard.NotNull(schemaRepository);

        string schemaId = _jsonApiSchemaIdSelector.GetRelationshipIdentifierSchemaId(relationship);

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

        OpenApiSchema referenceSchemaForResourceType = _resourceTypeSchemaGenerator.GenerateSchema(relationship.LeftType, schemaRepository);
        fullSchemaForIdentifier.Properties[JsonApiPropertyName.Type] = referenceSchemaForResourceType.WrapInExtendedSchema();

        OpenApiSchema referenceSchemaForRelationshipName = _relationshipNameSchemaGenerator.GenerateSchema(relationship, schemaRepository);
        fullSchemaForIdentifier.Properties[JsonApiPropertyName.Relationship] = referenceSchemaForRelationshipName.WrapInExtendedSchema();

#if NET6_0
        fullSchemaForIdentifier.ReorderProperties(RelationshipIdentifierPropertyNamesInOrder);
#endif

        schemaRepository.ReplaceSchemaId(relationshipIdentifierConstructedType, schemaId);
        referenceSchemaForIdentifier.Reference.Id = schemaId;

        return referenceSchemaForIdentifier;
    }
}
