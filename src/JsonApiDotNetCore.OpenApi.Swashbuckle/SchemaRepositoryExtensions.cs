using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal static class SchemaRepositoryExtensions
{
    public static OpenApiSchemaReference LookupByType(this SchemaRepository schemaRepository, Type schemaType)
    {
        ArgumentNullException.ThrowIfNull(schemaRepository);
        ArgumentNullException.ThrowIfNull(schemaType);

        if (!schemaRepository.TryLookupByTypeSafe(schemaType, out OpenApiSchemaReference? referenceSchema))
        {
            throw new InvalidOperationException($"Reference schema for '{schemaType.Name}' does not exist.");
        }

        return referenceSchema;
    }

    public static bool TryLookupByTypeSafe(this SchemaRepository schemaRepository, Type type, [NotNullWhen(true)] out OpenApiSchemaReference? referenceSchema)
    {
        bool result = schemaRepository.TryLookupByType(type, out OpenApiSchemaReference? obliviousReferenceSchema);
        referenceSchema = result ? obliviousReferenceSchema : null;
        return result;
    }
}
