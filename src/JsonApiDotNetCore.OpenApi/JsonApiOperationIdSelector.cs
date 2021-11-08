using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Humanizer;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.RelationshipData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;

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
        private readonly JsonNamingPolicy? _namingPolicy;
        private readonly ResourceNameFormatter _formatter;

        public JsonApiOperationIdSelector(IControllerResourceMapping controllerResourceMapping, JsonNamingPolicy? namingPolicy)
        {
            ArgumentGuard.NotNull(controllerResourceMapping, nameof(controllerResourceMapping));

            _controllerResourceMapping = controllerResourceMapping;
            _namingPolicy = namingPolicy;
            _formatter = new ResourceNameFormatter(namingPolicy);
        }

        public string GetOperationId(ApiDescription endpoint)
        {
            ArgumentGuard.NotNull(endpoint, nameof(endpoint));

            ResourceType primaryResourceType =
                _controllerResourceMapping.GetResourceTypeForController(endpoint.ActionDescriptor.GetActionMethod().ReflectedType)!;

            string template = GetTemplate(primaryResourceType.ClrType, endpoint);

            return ApplyTemplate(template, primaryResourceType.ClrType, endpoint);
        }

        private static string GetTemplate(Type primaryResourceType, ApiDescription endpoint)
        {
            Type requestDocumentType = GetDocumentType(primaryResourceType, endpoint);

            return DocumentOpenTypeToOperationIdTemplateMap[requestDocumentType];
        }

        private static Type GetDocumentType(Type primaryResourceType, ApiDescription endpoint)
        {
            var producesResponseTypeAttribute = endpoint.ActionDescriptor.GetFilterMetadata<ProducesResponseTypeAttribute>()!;
            ControllerParameterDescriptor? requestBodyDescriptor = endpoint.ActionDescriptor.GetBodyParameterDescriptor();

            Type documentType = requestBodyDescriptor?.ParameterType.GetGenericTypeDefinition() ??
                GetGenericTypeDefinitionOrDefault(producesResponseTypeAttribute.Type) ?? producesResponseTypeAttribute.Type;

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

        private static Type? GetGenericTypeDefinitionOrDefault(Type type)
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

            return _namingPolicy != null ? _namingPolicy.ConvertName(pascalCaseId) : pascalCaseId;
        }
    }
}
