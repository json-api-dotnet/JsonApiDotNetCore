using System.Text.Json;
using Humanizer;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.ActionMethods;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Relationships;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal sealed class OpenApiOperationIdSelector
{
    private const string ResourceIdTemplate = "[Method] [PrimaryResourceName]";
    private const string ResourceCollectionIdTemplate = $"{ResourceIdTemplate} Collection";
    private const string SecondaryResourceIdTemplate = $"{ResourceIdTemplate} [RelationshipName]";
    private const string RelationshipIdTemplate = $"{SecondaryResourceIdTemplate} Relationship";
    private const string AtomicOperationsIdTemplate = "[Method] Operations";

    private static readonly Dictionary<Type, string> SchemaOpenTypeToOpenApiOperationIdTemplateMap = new()
    {
        [typeof(CollectionResponseDocument<>)] = ResourceCollectionIdTemplate,
        [typeof(PrimaryResponseDocument<>)] = ResourceIdTemplate,
        [typeof(CreateRequestDocument<>)] = ResourceIdTemplate,
        [typeof(UpdateRequestDocument<>)] = ResourceIdTemplate,
        [typeof(void)] = ResourceIdTemplate,
        [typeof(SecondaryResponseDocument<>)] = SecondaryResourceIdTemplate,
        [typeof(NullableSecondaryResponseDocument<>)] = SecondaryResourceIdTemplate,
        [typeof(IdentifierCollectionResponseDocument<>)] = RelationshipIdTemplate,
        [typeof(IdentifierResponseDocument<>)] = RelationshipIdTemplate,
        [typeof(NullableIdentifierResponseDocument<>)] = RelationshipIdTemplate,
        [typeof(ToOneInRequest<>)] = RelationshipIdTemplate,
        [typeof(NullableToOneInRequest<>)] = RelationshipIdTemplate,
        [typeof(ToManyInRequest<>)] = RelationshipIdTemplate,
        [typeof(OperationsRequestDocument)] = AtomicOperationsIdTemplate
    };

    private static readonly Func<ApiDescription, string> DefaultOperationIdSelector = new SwaggerGeneratorOptions().OperationIdSelector;

    private readonly IControllerResourceMapping _controllerResourceMapping;
    private readonly IJsonApiOptions _options;

    public OpenApiOperationIdSelector(IControllerResourceMapping controllerResourceMapping, IJsonApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(controllerResourceMapping);
        ArgumentNullException.ThrowIfNull(options);

        _controllerResourceMapping = controllerResourceMapping;
        _options = options;
    }

    public string GetOpenApiOperationId(ApiDescription endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        var actionMethod = JsonApiActionMethod.TryCreate(endpoint.ActionDescriptor);

        if (actionMethod is not null and not CustomResourceActionMethod)
        {
            ResourceType? primaryResourceType = _controllerResourceMapping.GetResourceTypeForController(actionMethod.ControllerType);

            string template = GetTemplate(endpoint);
            return ApplyTemplate(template, primaryResourceType, endpoint);
        }

        return DefaultOperationIdSelector(endpoint);
    }

    private static string GetTemplate(ApiDescription endpoint)
    {
        Type documentType = GetDocumentType(endpoint);
        ConsistencyGuard.ThrowIf(!SchemaOpenTypeToOpenApiOperationIdTemplateMap.TryGetValue(documentType, out string? template));
        return template;
    }

    private static Type GetDocumentType(ApiDescription endpoint)
    {
        ProducesResponseTypeAttribute? producesResponseTypeAttribute = endpoint.ActionDescriptor.FilterDescriptors
            .Select(filterDescriptor => filterDescriptor.Filter).OfType<ProducesResponseTypeAttribute>().FirstOrDefault();

        ConsistencyGuard.ThrowIf(producesResponseTypeAttribute == null);

        ControllerParameterDescriptor? requestBodyDescriptor = endpoint.ActionDescriptor.GetBodyParameterDescriptor();
        Type documentOpenType = (requestBodyDescriptor?.ParameterType ?? producesResponseTypeAttribute.Type).ConstructedToOpenType();

        if (documentOpenType == typeof(CollectionResponseDocument<>) && endpoint.ParameterDescriptions.Count > 0)
        {
            documentOpenType = typeof(SecondaryResponseDocument<>);
        }

        return documentOpenType;
    }

    private string ApplyTemplate(string openApiOperationIdTemplate, ResourceType? resourceType, ApiDescription endpoint)
    {
        ConsistencyGuard.ThrowIf(endpoint.RelativePath == null);
        ConsistencyGuard.ThrowIf(endpoint.HttpMethod == null);

        string method = endpoint.HttpMethod.ToLowerInvariant();
        string relationshipName = openApiOperationIdTemplate.Contains("[RelationshipName]") ? endpoint.RelativePath.Split('/').Last() : string.Empty;

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:wrap_before_first_method_call true

        string pascalCaseOpenApiOperationId = openApiOperationIdTemplate
            .Replace("[Method]", method)
            .Replace("[PrimaryResourceName]", resourceType?.PublicName.Singularize())
            .Replace("[RelationshipName]", relationshipName)
            .Pascalize();

        // @formatter:wrap_before_first_method_call true restore
        // @formatter:wrap_chained_method_calls restore

        JsonNamingPolicy? namingPolicy = _options.SerializerOptions.PropertyNamingPolicy;
        return namingPolicy != null ? namingPolicy.ConvertName(pascalCaseOpenApiOperationId) : pascalCaseOpenApiOperationId;
    }
}
