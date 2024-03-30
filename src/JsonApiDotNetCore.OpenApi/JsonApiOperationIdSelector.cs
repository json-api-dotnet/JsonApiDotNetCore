using System.Reflection;
using System.Text.Json;
using Humanizer;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.JsonApiObjects.Relationships;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace JsonApiDotNetCore.OpenApi;

internal sealed class JsonApiOperationIdSelector
{
    private const string ResourceOperationIdTemplate = "[Method] [PrimaryResourceName]";
    private const string ResourceCollectionOperationIdTemplate = $"{ResourceOperationIdTemplate} Collection";
    private const string SecondaryOperationIdTemplate = $"{ResourceOperationIdTemplate} [RelationshipName]";
    private const string RelationshipOperationIdTemplate = $"{SecondaryOperationIdTemplate} Relationship";

    private static readonly IDictionary<Type, string> DocumentOpenTypeToOperationIdTemplateMap = new Dictionary<Type, string>
    {
        [typeof(ResourceCollectionResponseDocument<>)] = ResourceCollectionOperationIdTemplate,
        [typeof(PrimaryResourceResponseDocument<>)] = ResourceOperationIdTemplate,
        [typeof(ResourcePostRequestDocument<>)] = ResourceOperationIdTemplate,
        [typeof(ResourcePatchRequestDocument<>)] = ResourceOperationIdTemplate,
        [typeof(void)] = ResourceOperationIdTemplate,
        [typeof(SecondaryResourceResponseDocument<>)] = SecondaryOperationIdTemplate,
        [typeof(NullableSecondaryResourceResponseDocument<>)] = SecondaryOperationIdTemplate,
        [typeof(ResourceIdentifierCollectionResponseDocument<>)] = RelationshipOperationIdTemplate,
        [typeof(ResourceIdentifierResponseDocument<>)] = RelationshipOperationIdTemplate,
        [typeof(NullableResourceIdentifierResponseDocument<>)] = RelationshipOperationIdTemplate,
        [typeof(ToOneRelationshipInRequest<>)] = RelationshipOperationIdTemplate,
        [typeof(NullableToOneRelationshipInRequest<>)] = RelationshipOperationIdTemplate,
        [typeof(ToManyRelationshipInRequest<>)] = RelationshipOperationIdTemplate
    };

    private readonly IControllerResourceMapping _controllerResourceMapping;
    private readonly IJsonApiOptions _options;

    public JsonApiOperationIdSelector(IControllerResourceMapping controllerResourceMapping, IJsonApiOptions options)
    {
        ArgumentGuard.NotNull(controllerResourceMapping);
        ArgumentGuard.NotNull(options);

        _controllerResourceMapping = controllerResourceMapping;
        _options = options;
    }

    public string GetOperationId(ApiDescription endpoint)
    {
        ArgumentGuard.NotNull(endpoint);

        MethodInfo actionMethod = endpoint.ActionDescriptor.GetActionMethod();
        ResourceType? primaryResourceType = _controllerResourceMapping.GetResourceTypeForController(actionMethod.ReflectedType);

        if (primaryResourceType == null)
        {
            throw new UnreachableCodeException();
        }

        string template = GetTemplate(endpoint);

        return ApplyTemplate(template, primaryResourceType, endpoint);
    }

    private static string GetTemplate(ApiDescription endpoint)
    {
        Type requestDocumentType = GetDocumentType(endpoint);

        if (!DocumentOpenTypeToOperationIdTemplateMap.TryGetValue(requestDocumentType, out string? template))
        {
            throw new UnreachableCodeException();
        }

        return template;
    }

    private static Type GetDocumentType(ApiDescription endpoint)
    {
        var producesResponseTypeAttribute = endpoint.ActionDescriptor.GetFilterMetadata<ProducesResponseTypeAttribute>();

        if (producesResponseTypeAttribute == null)
        {
            throw new UnreachableCodeException();
        }

        ControllerParameterDescriptor? requestBodyDescriptor = endpoint.ActionDescriptor.GetBodyParameterDescriptor();
        Type documentType = (requestBodyDescriptor?.ParameterType ?? producesResponseTypeAttribute.Type).ConstructedToOpenType();

        if (documentType == typeof(ResourceCollectionResponseDocument<>) && endpoint.ParameterDescriptions.Count > 0)
        {
            documentType = typeof(SecondaryResourceResponseDocument<>);
        }

        return documentType;
    }

    private string ApplyTemplate(string operationIdTemplate, ResourceType resourceType, ApiDescription endpoint)
    {
        if (endpoint.RelativePath == null || endpoint.HttpMethod == null)
        {
            throw new UnreachableCodeException();
        }

        string method = endpoint.HttpMethod.ToLowerInvariant();
        string relationshipName = operationIdTemplate.Contains("[RelationshipName]") ? endpoint.RelativePath.Split("/").Last() : string.Empty;

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:wrap_before_first_method_call true

        string pascalCaseOperationId = operationIdTemplate
            .Replace("[Method]", method)
            .Replace("[PrimaryResourceName]", resourceType.PublicName.Singularize())
            .Replace("[RelationshipName]", relationshipName)
            .ToPascalCase();

        // @formatter:wrap_before_first_method_call true restore
        // @formatter:wrap_chained_method_calls restore

        JsonNamingPolicy? namingPolicy = _options.SerializerOptions.PropertyNamingPolicy;
        return namingPolicy != null ? namingPolicy.ConvertName(pascalCaseOperationId) : pascalCaseOperationId;
    }
}
