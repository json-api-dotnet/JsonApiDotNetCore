using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents
{
    internal sealed class ResourceTypeSchemaGenerator
    {
        private readonly ISchemaRepositoryAccessor _schemaRepositoryAccessor;
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly Dictionary<Type, OpenApiSchema> _resourceTypeSchemaCache = new();

        public ResourceTypeSchemaGenerator(ISchemaRepositoryAccessor schemaRepositoryAccessor, IResourceContextProvider resourceContextProvider)
        {
            ArgumentGuard.NotNull(schemaRepositoryAccessor, nameof(schemaRepositoryAccessor));
            ArgumentGuard.NotNull(resourceContextProvider, nameof(resourceContextProvider));

            _schemaRepositoryAccessor = schemaRepositoryAccessor;
            _resourceContextProvider = resourceContextProvider;
        }

        public OpenApiSchema Get(Type resourceType)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            if (_resourceTypeSchemaCache.TryGetValue(resourceType, out OpenApiSchema referenceSchema))
            {
                return referenceSchema;
            }

            ResourceContext resourceContext = _resourceContextProvider.GetResourceContext(resourceType);

            var fullSchema = new OpenApiSchema
            {
                Type = "string",
                Enum = new List<IOpenApiAny>
                {
                    new OpenApiString(resourceContext.PublicName)
                }
            };

            referenceSchema = new OpenApiSchema
            {
                Reference = new OpenApiReference
                {
                    Id = $"{resourceContext.PublicName}-resource-type",
                    Type = ReferenceType.Schema
                }
            };

            _schemaRepositoryAccessor.Current.AddDefinition(referenceSchema.Reference.Id, fullSchema);
            _resourceTypeSchemaCache.Add(resourceContext.ResourceType, referenceSchema);

            return referenceSchema;
        }
    }
}
