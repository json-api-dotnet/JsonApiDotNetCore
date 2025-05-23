using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal static class SchemaRepositoryExtensions
{
    private const string ReservedIdsFieldName = "_reservedIds";
    private static readonly FieldInfo ReservedIdsField = GetReservedIdsField();

    private static FieldInfo GetReservedIdsField()
    {
        FieldInfo? field = typeof(SchemaRepository).GetField(ReservedIdsFieldName, BindingFlags.Instance | BindingFlags.NonPublic);

        if (field == null)
        {
            throw new InvalidOperationException($"Failed to locate private field '{ReservedIdsFieldName}' " +
                $"in type '{typeof(SchemaRepository).FullName}' in assembly '{typeof(SchemaRepository).Assembly.FullName}'.");
        }

        if (field.FieldType != typeof(Dictionary<Type, string>))
        {
            throw new InvalidOperationException($"Unexpected type '{field.FieldType}' of private field '{ReservedIdsFieldName}' " +
                $"in type '{typeof(SchemaRepository).FullName}' in assembly '{typeof(SchemaRepository).Assembly.FullName}'.");
        }

        return field;
    }

    public static OpenApiSchema LookupByType(this SchemaRepository schemaRepository, Type schemaType)
    {
        ArgumentNullException.ThrowIfNull(schemaRepository);
        ArgumentNullException.ThrowIfNull(schemaType);

        if (!schemaRepository.TryLookupByType(schemaType, out OpenApiSchema? referenceSchema))
        {
            throw new InvalidOperationException($"Reference schema for '{schemaType.Name}' does not exist.");
        }

        return referenceSchema;
    }

    public static void ReplaceSchemaId(this SchemaRepository schemaRepository, Type oldSchemaType, string newSchemaId)
    {
        ArgumentNullException.ThrowIfNull(schemaRepository);
        ArgumentNullException.ThrowIfNull(oldSchemaType);
        ArgumentException.ThrowIfNullOrEmpty(newSchemaId);

        if (schemaRepository.TryLookupByType(oldSchemaType, out OpenApiSchema? referenceSchema))
        {
            string oldSchemaId = referenceSchema.Reference.Id;

            OpenApiSchema fullSchema = schemaRepository.Schemas[oldSchemaId];

            schemaRepository.Schemas.Remove(oldSchemaId);
            schemaRepository.Schemas.Add(newSchemaId, fullSchema);

            var reservedIds = (Dictionary<Type, string>)ReservedIdsField.GetValue(schemaRepository)!;
            reservedIds.Remove(oldSchemaType);
        }
    }
}
