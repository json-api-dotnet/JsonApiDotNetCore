using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.RelationshipData;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Newtonsoft.Json.Serialization;

namespace JsonApiDotNetCore.OpenApi
{
    internal sealed class JsonApiOperationIdSelector
    {
        private const string ResourceOperationIdTemplate = "[Method] [PrimaryResourceName]";
        private const string ResourceCollectionOperationIdTemplate = ResourceOperationIdTemplate + " Collection";
        private const string SecondaryOperationIdTemplate = ResourceOperationIdTemplate + " [RelationshipName]";
        private const string RelationshipOperationIdTemplate = SecondaryOperationIdTemplate + " Relationship";

        private static readonly IDictionary<Type, string> DocumentOpenTypeToOperationIdTemplateMap = new Dictionary<Type, string>
        {
            [typeof(ResourceCollectionResponseDocument<>)] = ResourceCollectionOperationIdTemplate,
            [typeof(PrimaryResourceResponseDocument<>)] = ResourceOperationIdTemplate,
            [typeof(ResourcePostRequestDocument<>)] = ResourceOperationIdTemplate,
            [typeof(ResourcePatchRequestDocument<>)] = ResourceOperationIdTemplate,
            [typeof(void)] = ResourceOperationIdTemplate,
            [typeof(SecondaryResourceResponseDocument<>)] = SecondaryOperationIdTemplate,
            [typeof(ResourceIdentifierCollectionResponseDocument<>)] = RelationshipOperationIdTemplate,
            [typeof(ResourceIdentifierResponseDocument<>)] = RelationshipOperationIdTemplate,
            [typeof(ToOneRelationshipRequestData<>)] = RelationshipOperationIdTemplate,
            [typeof(ToManyRelationshipRequestData<>)] = RelationshipOperationIdTemplate
        };

        private readonly IControllerResourceMapping _controllerResourceMapping;
        private readonly NamingStrategy _namingStrategy;
        private readonly ResourceNameFormatterProxy _formatter;

        public JsonApiOperationIdSelector(IControllerResourceMapping controllerResourceMapping, NamingStrategy namingStrategy)
        {
            ArgumentGuard.NotNull(controllerResourceMapping, nameof(controllerResourceMapping));
            ArgumentGuard.NotNull(namingStrategy, nameof(namingStrategy));

            _controllerResourceMapping = controllerResourceMapping;
            _namingStrategy = namingStrategy;
            _formatter = new ResourceNameFormatterProxy(namingStrategy);
        }

        public string GetOperationId(ApiDescription endpoint)
        {
            ArgumentGuard.NotNull(endpoint, nameof(endpoint));

            Type primaryResourceType = _controllerResourceMapping.GetResourceTypeForController(endpoint.ActionDescriptor.GetActionMethod().ReflectedType);

            string template = GetTemplate(primaryResourceType, endpoint);

            return ApplyTemplate(template, primaryResourceType, endpoint);
        }

        private static string GetTemplate(Type primaryResourceType, ApiDescription endpoint)
        {
            Type requestDocumentType = GetDocumentType(primaryResourceType, endpoint);

            return DocumentOpenTypeToOperationIdTemplateMap[requestDocumentType];
        }

        private static Type GetDocumentType(Type primaryResourceType, ApiDescription endpoint)
        {
            ControllerParameterDescriptor requestBodyDescriptor = endpoint.ActionDescriptor.GetBodyParameterDescriptor();
            var producesResponseTypeAttribute = endpoint.ActionDescriptor.GetFilterMetadata<ProducesResponseTypeAttribute>();

            Type documentType = requestBodyDescriptor?.ParameterType.GetGenericTypeDefinition() ??
                TryGetGenericTypeDefinition(producesResponseTypeAttribute.Type) ?? producesResponseTypeAttribute.Type;

            if (documentType == typeof(ResourceCollectionResponseDocument<>))
            {
                Type documentResourceType = producesResponseTypeAttribute.Type.GetGenericArguments()[0];

                if (documentResourceType != primaryResourceType)
                {
                    documentType = typeof(SecondaryResourceResponseDocument<>);
                }
            }

            return documentType;
        }

        private static Type TryGetGenericTypeDefinition(Type type)
        {
            return type.IsConstructedGenericType ? type.GetGenericTypeDefinition() : null;
        }

        private string ApplyTemplate(string operationIdTemplate, Type primaryResourceType, ApiDescription endpoint)
        {
            string method = endpoint.HttpMethod!.ToLowerInvariant();
            string primaryResourceName = _formatter.FormatResourceName(primaryResourceType).Singularize();
            string relationshipName = operationIdTemplate.Contains("[RelationshipName]") ? endpoint.RelativePath.Split("/").Last() : string.Empty;

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            string pascalCaseId = operationIdTemplate
                .Replace("[Method]", method)
                .Replace("[PrimaryResourceName]", primaryResourceName)
                .Replace("[RelationshipName]", relationshipName);

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            return _namingStrategy.GetPropertyName(pascalCaseId, false);
        }
    }
}
