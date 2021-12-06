using System.Text.Json;
using Microsoft.OpenApi.Models;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

internal sealed class NullableReferenceSchemaGenerator
{
    private const string PascalCaseNullableSchemaReferenceId = "NullValue";

    private readonly NullableReferenceSchemaStrategy _nullableReferenceStrategy =
        Enum.Parse<NullableReferenceSchemaStrategy>(NullableReferenceSchemaStrategy.Implicit.ToString());

    private readonly string _nullableSchemaReferenceId;
    private readonly ISchemaRepositoryAccessor _schemaRepositoryAccessor;

    private OpenApiSchema? _referenceSchemaForExplicitNullValue;

    public NullableReferenceSchemaGenerator(ISchemaRepositoryAccessor schemaRepositoryAccessor, JsonNamingPolicy? namingPolicy)
    {
        ArgumentGuard.NotNull(schemaRepositoryAccessor, nameof(schemaRepositoryAccessor));

        _schemaRepositoryAccessor = schemaRepositoryAccessor;

        _nullableSchemaReferenceId = namingPolicy != null ? namingPolicy.ConvertName(PascalCaseNullableSchemaReferenceId) : PascalCaseNullableSchemaReferenceId;
    }

    public OpenApiSchema GenerateSchema(OpenApiSchema referenceSchema)
    {
        ArgumentGuard.NotNull(referenceSchema, nameof(referenceSchema));

        return new OpenApiSchema
        {
            OneOf = new List<OpenApiSchema>
            {
                referenceSchema,
                _nullableReferenceStrategy == NullableReferenceSchemaStrategy.Explicit ? GetExplicitNullSchema() : GetImplicitNullSchema()
            }
        };
    }

    // This approach is supported in OAS starting from v3.1. See https://github.com/OAI/OpenAPI-Specification/issues/1368#issuecomment-580103688
    private static OpenApiSchema GetExplicitNullSchema()
    {
        return new OpenApiSchema
        {
            Type = "null"
        };
    }

    // This approach is supported starting from OAS v3.0. See https://github.com/OAI/OpenAPI-Specification/issues/1368#issuecomment-487314681
    private OpenApiSchema GetImplicitNullSchema()
    {
        EnsureFullSchemaForNullValueExists();

        return _referenceSchemaForExplicitNullValue ??= new OpenApiSchema
        {
            Reference = new OpenApiReference
            {
                Id = _nullableSchemaReferenceId,
                Type = ReferenceType.Schema
            }
        };
    }

    private void EnsureFullSchemaForNullValueExists()
    {
        if (!_schemaRepositoryAccessor.Current.Schemas.ContainsKey(_nullableSchemaReferenceId))
        {
            var fullSchemaForNullValue = new OpenApiSchema
            {
                Nullable = true,
                Not = new OpenApiSchema
                {
                    AnyOf = new List<OpenApiSchema>
                    {
                        new()
                        {
                            Type = "string"
                        },
                        new()
                        {
                            Type = "number"
                        },
                        new()
                        {
                            Type = "boolean"
                        },
                        new()
                        {
                            Type = "object"
                        },
                        new()
                        {
                            Type = "array"
                        }
                    },
                    Items = new OpenApiSchema()
                }
            };

            _schemaRepositoryAccessor.Current.AddDefinition(_nullableSchemaReferenceId, fullSchemaForNullValue);
        }
    }
}
