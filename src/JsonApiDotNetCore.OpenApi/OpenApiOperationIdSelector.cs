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

internal sealed class OpenApiOperationIdSelector
{
    private const string ResourceIdTemplate = "[Method] [PrimaryResourceName]";
    private const string ResourceCollectionIdTemplate = $"{ResourceIdTemplate} Collection";
    private const string SecondaryResourceIdTemplate = $"{ResourceIdTemplate} [RelationshipName]";
    private const string RelationshipIdTemplate = $"{SecondaryResourceIdTemplate} Relationship";
    private const string AtomicOperationsIdTemplate = "[Method] Operations";

    private static readonly IDictionary<Type, string> SchemaOpenTypeToOpenApiOperationIdTemplateMap = new Dictionary<Type, string>
    {
        [typeof(ResourceCollectionResponseDocument<>)] = ResourceCollectionIdTemplate,
        [typeof(PrimaryResourceResponseDocument<>)] = ResourceIdTemplate,
        [typeof(CreateResourceRequestDocument<>)] = ResourceIdTemplate,
        [typeof(UpdateResourceRequestDocument<>)] = ResourceIdTemplate,
        [typeof(void)] = ResourceIdTemplate,
        [typeof(SecondaryResourceResponseDocument<>)] = SecondaryResourceIdTemplate,
        [typeof(NullableSecondaryResourceResponseDocument<>)] = SecondaryResourceIdTemplate,
        [typeof(ResourceIdentifierCollectionResponseDocument<>)] = RelationshipIdTemplate,
        [typeof(ResourceIdentifierResponseDocument<>)] = RelationshipIdTemplate,
        [typeof(NullableResourceIdentifierResponseDocument<>)] = RelationshipIdTemplate,
        [typeof(ToOneRelationshipInRequest<>)] = RelationshipIdTemplate,
        [typeof(NullableToOneRelationshipInRequest<>)] = RelationshipIdTemplate,
        [typeof(ToManyRelationshipInRequest<>)] = RelationshipIdTemplate,
        [typeof(OperationsRequestDocument)] = AtomicOperationsIdTemplate
    };

    private readonly IControllerResourceMapping _controllerResourceMapping;
    private readonly IJsonApiOptions _options;

    public OpenApiOperationIdSelector(IControllerResourceMapping controllerResourceMapping, IJsonApiOptions options)
    {
        ArgumentGuard.NotNull(controllerResourceMapping);
        ArgumentGuard.NotNull(options);

        _controllerResourceMapping = controllerResourceMapping;
        _options = options;
    }

    public string GetOpenApiOperationId(ApiDescription endpoint)
    {
        ArgumentGuard.NotNull(endpoint);

        MethodInfo actionMethod = endpoint.ActionDescriptor.GetActionMethod();
        ResourceType? primaryResourceType = _controllerResourceMapping.GetResourceTypeForController(actionMethod.ReflectedType);

        string template = GetTemplate(endpoint);
        return ApplyTemplate(template, primaryResourceType, endpoint);
    }

    private static string GetTemplate(ApiDescription endpoint)
    {
        Type bodyType = GetBodyType(endpoint);

        if (!SchemaOpenTypeToOpenApiOperationIdTemplateMap.TryGetValue(bodyType, out string? template))
        {
            throw new UnreachableCodeException();
        }

        return template;
    }

    private static Type GetBodyType(ApiDescription endpoint)
    {
        var producesResponseTypeAttribute = endpoint.ActionDescriptor.GetFilterMetadata<ProducesResponseTypeAttribute>();

        if (producesResponseTypeAttribute == null)
        {
            throw new UnreachableCodeException();
        }

        ControllerParameterDescriptor? requestBodyDescriptor = endpoint.ActionDescriptor.GetBodyParameterDescriptor();
        Type bodyType = (requestBodyDescriptor?.ParameterType ?? producesResponseTypeAttribute.Type).ConstructedToOpenType();

        if (bodyType == typeof(ResourceCollectionResponseDocument<>) && endpoint.ParameterDescriptions.Count > 0)
        {
            bodyType = typeof(SecondaryResourceResponseDocument<>);
        }

        return bodyType;
    }

    private string ApplyTemplate(string openApiOperationIdTemplate, ResourceType? resourceType, ApiDescription endpoint)
    {
        if (endpoint.RelativePath == null || endpoint.HttpMethod == null)
        {
            throw new UnreachableCodeException();
        }

        string method = endpoint.HttpMethod.ToLowerInvariant();
        string relationshipName = openApiOperationIdTemplate.Contains("[RelationshipName]") ? endpoint.RelativePath.Split('/').Last() : string.Empty;

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:wrap_before_first_method_call true

        string pascalCaseOpenApiOperationId = openApiOperationIdTemplate
            .Replace("[Method]", method)
            .Replace("[PrimaryResourceName]", resourceType?.PublicName.Singularize())
            .Replace("[RelationshipName]", relationshipName)
            .ToPascalCase();

        // @formatter:wrap_before_first_method_call true restore
        // @formatter:wrap_chained_method_calls restore

        JsonNamingPolicy? namingPolicy = _options.SerializerOptions.PropertyNamingPolicy;
        return namingPolicy != null ? namingPolicy.ConvertName(pascalCaseOpenApiOperationId) : pascalCaseOpenApiOperationId;
    }
}
