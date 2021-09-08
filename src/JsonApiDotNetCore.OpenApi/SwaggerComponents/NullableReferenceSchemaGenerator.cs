using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents
{
    internal sealed class NullableReferenceSchemaGenerator
    {
        private static readonly NullableReferenceSchemaStrategy NullableReferenceStrategy =
            Enum.Parse<NullableReferenceSchemaStrategy>(NullableReferenceSchemaStrategy.Implicit.ToString());

        private static OpenApiSchema _referenceSchemaForNullValue;
        private readonly ISchemaRepositoryAccessor _schemaRepositoryAccessor;

        public NullableReferenceSchemaGenerator(ISchemaRepositoryAccessor schemaRepositoryAccessor)
        {
            ArgumentGuard.NotNull(schemaRepositoryAccessor, nameof(schemaRepositoryAccessor));

            _schemaRepositoryAccessor = schemaRepositoryAccessor;
        }

        public OpenApiSchema GenerateSchema(OpenApiSchema referenceSchema)
        {
            ArgumentGuard.NotNull(referenceSchema, nameof(referenceSchema));

            return new OpenApiSchema
            {
                OneOf = new List<OpenApiSchema>
                {
                    referenceSchema,
                    GetNullableReferenceSchema()
                }
            };
        }

        private OpenApiSchema GetNullableReferenceSchema()
        {
            return NullableReferenceStrategy == NullableReferenceSchemaStrategy.Explicit
                ? GetNullableReferenceSchemaUsingExplicitNullType()
                : GetNullableReferenceSchemaUsingImplicitNullType();
        }

        // This approach is supported in OAS starting from v3.1. See https://github.com/OAI/OpenAPI-Specification/issues/1368#issuecomment-580103688
        private static OpenApiSchema GetNullableReferenceSchemaUsingExplicitNullType()
        {
            return new()
            {
                Type = "null"
            };
        }

        // This approach is supported starting from OAS v3.0. See https://github.com/OAI/OpenAPI-Specification/issues/1368#issuecomment-487314681
        private OpenApiSchema GetNullableReferenceSchemaUsingImplicitNullType()
        {
            if (_referenceSchemaForNullValue != null)
            {
                return _referenceSchemaForNullValue;
            }

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

            _referenceSchemaForNullValue = _schemaRepositoryAccessor.Current.AddDefinition("null-value", fullSchemaForNullValue);

            return _referenceSchemaForNullValue;
        }
    }
}
