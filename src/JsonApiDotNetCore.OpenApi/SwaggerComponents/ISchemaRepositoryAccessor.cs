using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

internal interface ISchemaRepositoryAccessor
{
    SchemaRepository Current { get; }
}
