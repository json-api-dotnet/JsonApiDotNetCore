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
        private static readonly Type[] JsonApiDocumentOpenTypes =
        {
            typeof(ResourceCollectionResponseDocument<>),
            typeof(PrimaryResourceResponseDocument<>),
            typeof(SecondaryResourceResponseDocument<>),
            typeof(NullableSecondaryResourceResponseDocument<>),
            typeof(ResourcePostRequestDocument<>),
            typeof(ResourcePatchRequestDocument<>),
            typeof(ResourceIdentifierCollectionResponseDocument<>),
            typeof(ResourceIdentifierResponseDocument<>),
            typeof(NullableResourceIdentifierResponseDocument<>),
            typeof(ToManyRelationshipRequestData<>),
            typeof(ToOneRelationshipRequestData<>),
            typeof(NullableToOneRelationshipRequestData<>)
        };

        private readonly ISchemaGenerator _defaultSchemaGenerator;
        private readonly ResourceObjectSchemaGenerator _resourceObjectSchemaGenerator;
        private readonly NullableReferenceSchemaGenerator _nullableReferenceSchemaGenerator;
        private readonly SchemaRepositoryAccessor _schemaRepositoryAccessor = new();

        public JsonApiSchemaGenerator(SchemaGenerator defaultSchemaGenerator, IResourceGraph resourceGraph, IJsonApiOptions options)
        {
            ArgumentGuard.NotNull(defaultSchemaGenerator, nameof(defaultSchemaGenerator));
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));
            ArgumentGuard.NotNull(options, nameof(options));

            _defaultSchemaGenerator = defaultSchemaGenerator;
            _nullableReferenceSchemaGenerator = new NullableReferenceSchemaGenerator(_schemaRepositoryAccessor);
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

            if (IsJsonApiDocument(type))
            {
                OpenApiSchema schema = GenerateJsonApiDocumentSchema(type);

                if (IsDataPropertyNullable(type))
                {
                    SetDataObjectSchemaToNullable(schema);
                }
            }

            return _defaultSchemaGenerator.GenerateSchema(type, schemaRepository, memberInfo, parameterInfo);
        }

        private static bool IsJsonApiDocument(Type type)
        {
            return type.IsConstructedGenericType && JsonApiDocumentOpenTypes.Contains(type.GetGenericTypeDefinition());
        }

        private OpenApiSchema GenerateJsonApiDocumentSchema(Type documentType)
        {
            Type resourceObjectType = documentType.BaseType!.GenericTypeArguments[0];

            if (!_schemaRepositoryAccessor.Current.TryLookupByType(resourceObjectType, out OpenApiSchema referenceSchemaForResourceObject))
            {
                referenceSchemaForResourceObject = _resourceObjectSchemaGenerator.GenerateSchema(resourceObjectType);
            }

            OpenApiSchema referenceSchemaForDocument = _defaultSchemaGenerator.GenerateSchema(documentType, _schemaRepositoryAccessor.Current);
            OpenApiSchema fullSchemaForDocument = _schemaRepositoryAccessor.Current.Schemas[referenceSchemaForDocument.Reference.Id];

            OpenApiSchema referenceSchemaForDataObject = IsManyDataDocument(documentType)
                ? CreateArrayTypeDataSchema(referenceSchemaForResourceObject)
                : referenceSchemaForResourceObject;

            fullSchemaForDocument.Properties[JsonApiObjectPropertyName.Data] = referenceSchemaForDataObject;

            return referenceSchemaForDocument;
        }

        private static bool IsManyDataDocument(Type documentType)
        {
            return documentType.BaseType!.GetGenericTypeDefinition() == typeof(ManyData<>);
        }

        private static bool IsDataPropertyNullable(Type type)
        {
            PropertyInfo? dataProperty = type.GetProperty(nameof(JsonApiObjectPropertyName.Data));

            if (dataProperty == null)
            {
                throw new UnreachableCodeException();
            }

            return dataProperty.GetTypeCategory() == TypeCategory.NullableReferenceType;
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
    }
}
