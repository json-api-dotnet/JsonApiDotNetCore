using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

internal sealed class SchemaRepositoryAccessor : ISchemaRepositoryAccessor
{
    private SchemaRepository? _schemaRepository;

    public SchemaRepository Current
    {
        get
        {
            if (_schemaRepository == null)
            {
                throw new InvalidOperationException("SchemaRepository unavailable.");
            }

            return _schemaRepository;
        }
        set
        {
            ArgumentGuard.NotNull(value);

            _schemaRepository = value;
        }
    }
}
