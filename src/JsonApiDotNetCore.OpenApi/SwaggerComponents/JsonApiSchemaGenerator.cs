using System;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.OpenApi.JsonApiObjects;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.RelationshipData;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents
{
    internal sealed class JsonApiSchemaGenerator : ISchemaGenerator
    {
        private static readonly Type[] JsonApiResourceDocumentOpenTypes =
        {
            typeof(ResourceCollectionResponseDocument<>),
            typeof(PrimaryResourceResponseDocument<>),
            typeof(SecondaryResourceResponseDocument<>),
            typeof(ResourcePostRequestDocument<>),
            typeof(ResourcePatchRequestDocument<>)
        };

        private static readonly Type[] SingleNonPrimaryDataDocumentOpenTypes =
        {
            typeof(ToOneRelationshipRequestData<>),
            typeof(ResourceIdentifierResponseDocument<>),
            typeof(SecondaryResourceResponseDocument<>)
        };

        private static readonly Type[] JsonApiResourceIdentifierDocumentOpenTypes =
        {
            typeof(ResourceIdentifierCollectionResponseDocument<>),
            typeof(ResourceIdentifierResponseDocument<>)
        };

        private readonly ISchemaGenerator _defaultSchemaGenerator;
        private readonly ResourceObjectSchemaGenerator _resourceObjectSchemaGenerator;
        private readonly NullableReferenceSchemaGenerator _nullableReferenceSchemaGenerator;
        private readonly JsonApiObjectNullabilityProcessor _jsonApiObjectNullabilityProcessor;
        private readonly SchemaRepositoryAccessor _schemaRepositoryAccessor = new();

        public JsonApiSchemaGenerator(SchemaGenerator defaultSchemaGenerator, IResourceGraph resourceGraph, IJsonApiOptions options)
        {
            ArgumentGuard.NotNull(defaultSchemaGenerator, nameof(defaultSchemaGenerator));
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));
            ArgumentGuard.NotNull(options, nameof(options));

            _defaultSchemaGenerator = defaultSchemaGenerator;
            _nullableReferenceSchemaGenerator = new NullableReferenceSchemaGenerator(_schemaRepositoryAccessor);
            _jsonApiObjectNullabilityProcessor = new JsonApiObjectNullabilityProcessor(_schemaRepositoryAccessor);
            _resourceObjectSchemaGenerator = new ResourceObjectSchemaGenerator(defaultSchemaGenerator, resourceGraph, options, _schemaRepositoryAccessor);
        }

        public OpenApiSchema GenerateSchema(Type type, SchemaRepository schemaRepository, MemberInfo? memberInfo = null, ParameterInfo? parameterInfo = null)
        {
            ArgumentGuard.NotNull(type, nameof(type));
            ArgumentGuard.NotNull(schemaRepository, nameof(schemaRepository));

            _schemaRepositoryAccessor.Current = schemaRepository;

            if (schemaRepository.TryLookupByType(type, out OpenApiSchema jsonApiDocumentSchema))
            {
                return jsonApiDocumentSchema;
            }

            OpenApiSchema schema = IsJsonApiResourceDocument(type)
                ? GenerateResourceJsonApiDocumentSchema(type)
                : _defaultSchemaGenerator.GenerateSchema(type, schemaRepository, memberInfo, parameterInfo);

            if (IsSingleNonPrimaryDataDocument(type))
            {
                SetDataObjectSchemaToNullable(schema);
            }

            if (IsJsonApiDocument(type))
            {
                RemoveNotApplicableNullability(schema);
            }

            return schema;
        }

        private static bool IsJsonApiResourceDocument(Type type)
        {
            return type.IsConstructedGenericType && JsonApiResourceDocumentOpenTypes.Contains(type.GetGenericTypeDefinition());
        }

        private static bool IsJsonApiDocument(Type type)
        {
            return IsJsonApiResourceDocument(type) || IsJsonApiResourceIdentifierDocument(type);
        }

        private static bool IsJsonApiResourceIdentifierDocument(Type type)
        {
            return type.IsConstructedGenericType && JsonApiResourceIdentifierDocumentOpenTypes.Contains(type.GetGenericTypeDefinition());
        }

        private OpenApiSchema GenerateResourceJsonApiDocumentSchema(Type type)
        {
            Type resourceObjectType = type.BaseType!.GenericTypeArguments[0];

            if (!_schemaRepositoryAccessor.Current.TryLookupByType(resourceObjectType, out OpenApiSchema referenceSchemaForResourceObject))
            {
                referenceSchemaForResourceObject = _resourceObjectSchemaGenerator.GenerateSchema(resourceObjectType);
            }

            OpenApiSchema referenceSchemaForDocument = _defaultSchemaGenerator.GenerateSchema(type, _schemaRepositoryAccessor.Current);
            OpenApiSchema fullSchemaForDocument = _schemaRepositoryAccessor.Current.Schemas[referenceSchemaForDocument.Reference.Id];

            OpenApiSchema referenceSchemaForDataObject =
                IsSingleDataDocument(type) ? referenceSchemaForResourceObject : CreateArrayTypeDataSchema(referenceSchemaForResourceObject);

            fullSchemaForDocument.Properties[JsonApiObjectPropertyName.Data] = referenceSchemaForDataObject;

            return referenceSchemaForDocument;
        }

        private static bool IsSingleDataDocument(Type type)
        {
            return type.BaseType?.IsConstructedGenericType == true && type.BaseType.GetGenericTypeDefinition() == typeof(SingleData<>);
        }

        private static bool IsSingleNonPrimaryDataDocument(Type type)
        {
            return type.IsConstructedGenericType && SingleNonPrimaryDataDocumentOpenTypes.Contains(type.GetGenericTypeDefinition());
        }

        private void SetDataObjectSchemaToNullable(OpenApiSchema referenceSchemaForDocument)
        {
            OpenApiSchema fullSchemaForDocument = _schemaRepositoryAccessor.Current.Schemas[referenceSchemaForDocument.Reference.Id];
            OpenApiSchema referenceSchemaForData = fullSchemaForDocument.Properties[JsonApiObjectPropertyName.Data];
            fullSchemaForDocument.Properties[JsonApiObjectPropertyName.Data] = _nullableReferenceSchemaGenerator.GenerateSchema(referenceSchemaForData);
        }

        private static OpenApiSchema CreateArrayTypeDataSchema(OpenApiSchema referenceSchemaForResourceObject)
        {
            return new OpenApiSchema
            {
                Items = referenceSchemaForResourceObject,
                Type = "array"
            };
        }

        private void RemoveNotApplicableNullability(OpenApiSchema schema)
        {
            _jsonApiObjectNullabilityProcessor.ClearDocumentProperties(schema);
        }
    }
}
