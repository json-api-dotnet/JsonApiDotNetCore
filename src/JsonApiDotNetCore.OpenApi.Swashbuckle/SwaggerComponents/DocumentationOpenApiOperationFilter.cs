using System.Net;
using System.Reflection;
using Humanizer;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class DocumentationOpenApiOperationFilter : IOperationFilter
{
    private const string GetPrimaryName = nameof(BaseJsonApiController<Identifiable<int>, int>.GetAsync);
    private const string GetSecondaryName = nameof(BaseJsonApiController<Identifiable<int>, int>.GetSecondaryAsync);
    private const string GetRelationshipName = nameof(BaseJsonApiController<Identifiable<int>, int>.GetRelationshipAsync);
    private const string PostResourceName = nameof(BaseJsonApiController<Identifiable<int>, int>.PostAsync);
    private const string PostRelationshipName = nameof(BaseJsonApiController<Identifiable<int>, int>.PostRelationshipAsync);
    private const string PatchResourceName = nameof(BaseJsonApiController<Identifiable<int>, int>.PatchAsync);
    private const string PatchRelationshipName = nameof(BaseJsonApiController<Identifiable<int>, int>.PatchRelationshipAsync);
    private const string DeleteResourceName = nameof(BaseJsonApiController<Identifiable<int>, int>.DeleteAsync);
    private const string DeleteRelationshipName = nameof(BaseJsonApiController<Identifiable<int>, int>.DeleteRelationshipAsync);
    private const string PostOperationsName = nameof(BaseJsonApiOperationsController.PostOperationsAsync);

    private const string TextCompareETag =
        "Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.";

    private const string TextCompletedSuccessfully = "The operation completed successfully.";
    private const string TextNotModified = "The fingerprint of the HTTP response matches one of the ETags from the incoming If-None-Match header.";
    private const string TextQueryStringBad = "The query string is invalid.";
    private const string TextRequestBodyBad = "The request body is missing or malformed.";
    private const string TextQueryStringOrRequestBodyBad = "The query string is invalid or the request body is missing or malformed.";
    private const string TextConflict = "The request body contains conflicting information or another resource with the same ID already exists.";
    private const string TextRequestBodyIncompatibleIdOrType = "A resource type or identifier in the request body is incompatible.";
    private const string TextRequestBodyValidationFailed = "Validation of the request body failed.";
    private const string TextRequestBodyClientId = "Client-generated IDs cannot be used at this endpoint.";

    private const string ResourceQueryStringParameters =
        "For syntax, see the documentation for the [`include`](https://www.jsonapi.net/usage/reading/including-relationships.html)/" +
        "[`filter`](https://www.jsonapi.net/usage/reading/filtering.html)/[`sort`](https://www.jsonapi.net/usage/reading/sorting.html)/" +
        "[`page`](https://www.jsonapi.net/usage/reading/pagination.html)/" +
        "[`fields`](https://www.jsonapi.net/usage/reading/sparse-fieldset-selection.html) query string parameters.";

    private const string RelationshipQueryStringParameters = "For syntax, see the documentation for the " +
        "[`filter`](https://www.jsonapi.net/usage/reading/filtering.html)/[`sort`](https://www.jsonapi.net/usage/reading/sorting.html)/" +
        "[`page`](https://www.jsonapi.net/usage/reading/pagination.html)/" +
        "[`fields`](https://www.jsonapi.net/usage/reading/sparse-fieldset-selection.html) query string parameters.";

    private readonly IJsonApiOptions _options;
    private readonly IControllerResourceMapping _controllerResourceMapping;
    private readonly ResourceFieldValidationMetadataProvider _resourceFieldValidationMetadataProvider;

    public DocumentationOpenApiOperationFilter(IJsonApiOptions options, IControllerResourceMapping controllerResourceMapping,
        ResourceFieldValidationMetadataProvider resourceFieldValidationMetadataProvider)
    {
        ArgumentGuard.NotNull(options);
        ArgumentGuard.NotNull(controllerResourceMapping);
        ArgumentGuard.NotNull(resourceFieldValidationMetadataProvider);

        _options = options;
        _controllerResourceMapping = controllerResourceMapping;
        _resourceFieldValidationMetadataProvider = resourceFieldValidationMetadataProvider;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ArgumentGuard.NotNull(operation);
        ArgumentGuard.NotNull(context);

        bool hasHeadVerb = context.ApiDescription.HttpMethod == "HEAD";

        if (hasHeadVerb)
        {
            operation.Responses.Clear();
        }

        MethodInfo actionMethod = context.ApiDescription.ActionDescriptor.GetActionMethod();
        string actionName = context.MethodInfo.Name;
        ResourceType? resourceType = _controllerResourceMapping.GetResourceTypeForController(actionMethod.ReflectedType);

        if (resourceType != null)
        {
            switch (actionName)
            {
                case GetPrimaryName or PostResourceName or PatchResourceName or DeleteResourceName:
                {
                    switch (actionName)
                    {
                        case GetPrimaryName:
                        {
                            ApplyGetPrimary(operation, resourceType, hasHeadVerb);
                            break;
                        }
                        case PostResourceName:
                        {
                            ApplyPostResource(operation, resourceType);
                            break;
                        }
                        case PatchResourceName:
                        {
                            ApplyPatchResource(operation, resourceType);
                            break;
                        }
                        case DeleteResourceName:
                        {
                            ApplyDeleteResource(operation, resourceType);
                            break;
                        }
                    }

                    break;
                }
                case GetSecondaryName or GetRelationshipName or PostRelationshipName or PatchRelationshipName or DeleteRelationshipName:
                {
                    RelationshipAttribute relationship = GetRelationshipFromRoute(context.ApiDescription, resourceType);

                    switch (actionName)
                    {
                        case GetSecondaryName:
                        {
                            ApplyGetSecondary(operation, relationship, hasHeadVerb);
                            break;
                        }
                        case GetRelationshipName:
                        {
                            ApplyGetRelationship(operation, relationship, hasHeadVerb);
                            break;
                        }
                        case PostRelationshipName:
                        {
                            ApplyPostRelationship(operation, relationship);
                            break;
                        }
                        case PatchRelationshipName:
                        {
                            ApplyPatchRelationship(operation, relationship);
                            break;
                        }
                        case DeleteRelationshipName:
                        {
                            ApplyDeleteRelationship(operation, relationship);
                            break;
                        }
                    }

                    break;
                }
            }
        }
        else if (actionName == PostOperationsName)
        {
            ApplyPostOperations(operation);
        }
    }

    private static void ApplyGetPrimary(OpenApiOperation operation, ResourceType resourceType, bool hasHeadVerb)
    {
        if (operation.Parameters.Count == 0)
        {
            if (hasHeadVerb)
            {
                SetOperationSummary(operation, $"Retrieves a collection of {resourceType} without returning them.");
                SetOperationRemarks(operation, TextCompareETag);
                SetResponseDescription(operation.Responses, HttpStatusCode.OK, TextCompletedSuccessfully);
                SetResponseHeaderETag(operation.Responses, HttpStatusCode.OK);
                SetResponseHeaderContentLength(operation.Responses, HttpStatusCode.OK);
                SetResponseDescription(operation.Responses, HttpStatusCode.NotModified, TextNotModified);
                SetResponseHeaderETag(operation.Responses, HttpStatusCode.NotModified);
            }
            else
            {
                SetOperationSummary(operation, $"Retrieves a collection of {resourceType}.");

                SetResponseDescription(operation.Responses, HttpStatusCode.OK,
                    $"Successfully returns the found {resourceType}, or an empty array if none were found.");

                SetResponseHeaderETag(operation.Responses, HttpStatusCode.OK);
                SetResponseDescription(operation.Responses, HttpStatusCode.NotModified, TextNotModified);
                SetResponseHeaderETag(operation.Responses, HttpStatusCode.NotModified);
            }

            AddQueryStringParameters(operation, false);
            AddRequestHeaderIfNoneMatch(operation);
            SetResponseDescription(operation.Responses, HttpStatusCode.BadRequest, TextQueryStringBad);
        }
        else if (operation.Parameters.Count == 1)
        {
            string singularName = resourceType.PublicName.Singularize();

            if (hasHeadVerb)
            {
                SetOperationSummary(operation, $"Retrieves an individual {singularName} by its identifier without returning it.");
                SetOperationRemarks(operation, TextCompareETag);
                SetResponseDescription(operation.Responses, HttpStatusCode.OK, TextCompletedSuccessfully);
                SetResponseHeaderETag(operation.Responses, HttpStatusCode.OK);
                SetResponseHeaderContentLength(operation.Responses, HttpStatusCode.OK);
                SetResponseDescription(operation.Responses, HttpStatusCode.NotModified, TextNotModified);
                SetResponseHeaderETag(operation.Responses, HttpStatusCode.NotModified);
            }
            else
            {
                SetOperationSummary(operation, $"Retrieves an individual {singularName} by its identifier.");
                SetResponseDescription(operation.Responses, HttpStatusCode.OK, $"Successfully returns the found {singularName}.");
                SetResponseHeaderETag(operation.Responses, HttpStatusCode.OK);
                SetResponseDescription(operation.Responses, HttpStatusCode.NotModified, TextNotModified);
                SetResponseHeaderETag(operation.Responses, HttpStatusCode.NotModified);
            }

            SetParameterDescription(operation.Parameters[0], $"The identifier of the {singularName} to retrieve.");
            AddQueryStringParameters(operation, false);
            AddRequestHeaderIfNoneMatch(operation);
            SetResponseDescription(operation.Responses, HttpStatusCode.BadRequest, TextQueryStringBad);
            SetResponseDescription(operation.Responses, HttpStatusCode.NotFound, $"The {singularName} does not exist.");
        }
    }

    private void ApplyPostResource(OpenApiOperation operation, ResourceType resourceType)
    {
        string singularName = resourceType.PublicName.Singularize();

        SetOperationSummary(operation, $"Creates a new {singularName}.");
        AddQueryStringParameters(operation, false);
        SetRequestBodyDescription(operation.RequestBody, $"The attributes and relationships of the {singularName} to create.");

        SetResponseDescription(operation.Responses, HttpStatusCode.Created,
            $"The {singularName} was successfully created, which resulted in additional changes. The newly created {singularName} is returned.");

        SetResponseHeaderLocation(operation.Responses, HttpStatusCode.Created, singularName);

        SetResponseDescription(operation.Responses, HttpStatusCode.NoContent,
            $"The {singularName} was successfully created, which did not result in additional changes.");

        SetResponseDescription(operation.Responses, HttpStatusCode.BadRequest, TextQueryStringOrRequestBodyBad);

        ClientIdGenerationMode clientIdGeneration = resourceType.ClientIdGeneration ?? _options.ClientIdGeneration;

        if (clientIdGeneration == ClientIdGenerationMode.Forbidden)
        {
            SetResponseDescription(operation.Responses, HttpStatusCode.Forbidden, TextRequestBodyClientId);
        }

        SetResponseDescription(operation.Responses, HttpStatusCode.NotFound, "A related resource does not exist.");
        SetResponseDescription(operation.Responses, HttpStatusCode.Conflict, TextConflict);
        SetResponseDescription(operation.Responses, HttpStatusCode.UnprocessableEntity, TextRequestBodyValidationFailed);
    }

    private void ApplyPatchResource(OpenApiOperation operation, ResourceType resourceType)
    {
        string singularName = resourceType.PublicName.Singularize();

        SetOperationSummary(operation, $"Updates an existing {singularName}.");
        SetParameterDescription(operation.Parameters[0], $"The identifier of the {singularName} to update.");
        AddQueryStringParameters(operation, false);

        SetRequestBodyDescription(operation.RequestBody,
            $"The attributes and relationships of the {singularName} to update. Omitted fields are left unchanged.");

        SetResponseDescription(operation.Responses, HttpStatusCode.OK,
            $"The {singularName} was successfully updated, which resulted in additional changes. The updated {singularName} is returned.");

        SetResponseDescription(operation.Responses, HttpStatusCode.NoContent,
            $"The {singularName} was successfully updated, which did not result in additional changes.");

        SetResponseDescription(operation.Responses, HttpStatusCode.BadRequest, TextQueryStringOrRequestBodyBad);
        SetResponseDescription(operation.Responses, HttpStatusCode.NotFound, $"The {singularName} or a related resource does not exist.");
        SetResponseDescription(operation.Responses, HttpStatusCode.Conflict, TextRequestBodyIncompatibleIdOrType);
        SetResponseDescription(operation.Responses, HttpStatusCode.UnprocessableEntity, TextRequestBodyValidationFailed);
    }

    private void ApplyDeleteResource(OpenApiOperation operation, ResourceType resourceType)
    {
        string singularName = resourceType.PublicName.Singularize();

        SetOperationSummary(operation, $"Deletes an existing {singularName} by its identifier.");
        SetParameterDescription(operation.Parameters[0], $"The identifier of the {singularName} to delete.");
        SetResponseDescription(operation.Responses, HttpStatusCode.NoContent, $"The {singularName} was successfully deleted.");
        SetResponseDescription(operation.Responses, HttpStatusCode.NotFound, $"The {singularName} does not exist.");
    }

    private static void ApplyGetSecondary(OpenApiOperation operation, RelationshipAttribute relationship, bool hasHeadVerb)
    {
        string singularLeftName = relationship.LeftType.PublicName.Singularize();
        string rightName = relationship is HasOneAttribute ? relationship.RightType.PublicName.Singularize() : relationship.RightType.PublicName;

        if (hasHeadVerb)
        {
            SetOperationSummary(operation,
                relationship is HasOneAttribute
                    ? $"Retrieves the related {rightName} of an individual {singularLeftName}'s {relationship} relationship without returning it."
                    : $"Retrieves the related {rightName} of an individual {singularLeftName}'s {relationship} relationship without returning them.");

            SetOperationRemarks(operation, TextCompareETag);
            SetResponseDescription(operation.Responses, HttpStatusCode.OK, TextCompletedSuccessfully);
            SetResponseHeaderETag(operation.Responses, HttpStatusCode.OK);
            SetResponseHeaderContentLength(operation.Responses, HttpStatusCode.OK);
            SetResponseDescription(operation.Responses, HttpStatusCode.NotModified, TextNotModified);
            SetResponseHeaderETag(operation.Responses, HttpStatusCode.NotModified);
        }
        else
        {
            SetOperationSummary(operation, $"Retrieves the related {rightName} of an individual {singularLeftName}'s {relationship} relationship.");

            SetResponseDescription(operation.Responses, HttpStatusCode.OK,
                relationship is HasOneAttribute
                    ? $"Successfully returns the found {rightName}, or <c>null</c> if it was not found."
                    : $"Successfully returns the found {rightName}, or an empty array if none were found.");

            SetResponseHeaderETag(operation.Responses, HttpStatusCode.OK);
            SetResponseDescription(operation.Responses, HttpStatusCode.NotModified, TextNotModified);
            SetResponseHeaderETag(operation.Responses, HttpStatusCode.NotModified);
        }

        SetParameterDescription(operation.Parameters[0], $"The identifier of the {singularLeftName} whose related {rightName} to retrieve.");
        AddQueryStringParameters(operation, false);
        AddRequestHeaderIfNoneMatch(operation);
        SetResponseDescription(operation.Responses, HttpStatusCode.BadRequest, TextQueryStringBad);
        SetResponseDescription(operation.Responses, HttpStatusCode.NotFound, $"The {singularLeftName} does not exist.");
    }

    private static void ApplyGetRelationship(OpenApiOperation operation, RelationshipAttribute relationship, bool hasHeadVerb)
    {
        string singularLeftName = relationship.LeftType.PublicName.Singularize();
        string singularRightName = relationship.RightType.PublicName.Singularize();
        string ident = relationship is HasOneAttribute ? "identity" : "identities";

        if (hasHeadVerb)
        {
            SetOperationSummary(operation,
                relationship is HasOneAttribute
                    ? $"Retrieves the related {singularRightName} {ident} of an individual {singularLeftName}'s {relationship} relationship without returning it."
                    : $"Retrieves the related {singularRightName} {ident} of an individual {singularLeftName}'s {relationship} relationship without returning them.");

            SetOperationRemarks(operation, TextCompareETag);
            SetResponseDescription(operation.Responses, HttpStatusCode.OK, TextCompletedSuccessfully);
            SetResponseHeaderETag(operation.Responses, HttpStatusCode.OK);
            SetResponseHeaderContentLength(operation.Responses, HttpStatusCode.OK);
            SetResponseDescription(operation.Responses, HttpStatusCode.NotModified, TextNotModified);
            SetResponseHeaderETag(operation.Responses, HttpStatusCode.NotModified);
        }
        else
        {
            SetOperationSummary(operation,
                $"Retrieves the related {singularRightName} {ident} of an individual {singularLeftName}'s {relationship} relationship.");

            SetResponseDescription(operation.Responses, HttpStatusCode.OK,
                relationship is HasOneAttribute
                    ? $"Successfully returns the found {singularRightName} {ident}, or <c>null</c> if it was not found."
                    : $"Successfully returns the found {singularRightName} {ident}, or an empty array if none were found.");

            SetResponseHeaderETag(operation.Responses, HttpStatusCode.OK);
            SetResponseDescription(operation.Responses, HttpStatusCode.NotModified, TextNotModified);
            SetResponseHeaderETag(operation.Responses, HttpStatusCode.NotModified);
        }

        SetParameterDescription(operation.Parameters[0], $"The identifier of the {singularLeftName} whose related {singularRightName} {ident} to retrieve.");
        AddQueryStringParameters(operation, true);
        AddRequestHeaderIfNoneMatch(operation);
        SetResponseDescription(operation.Responses, HttpStatusCode.BadRequest, TextQueryStringBad);
        SetResponseDescription(operation.Responses, HttpStatusCode.NotFound, $"The {singularLeftName} does not exist.");
    }

    private void ApplyPostRelationship(OpenApiOperation operation, RelationshipAttribute relationship)
    {
        string singularLeftName = relationship.LeftType.PublicName.Singularize();
        string rightName = relationship.RightType.PublicName;

        SetOperationSummary(operation, $"Adds existing {rightName} to the {relationship} relationship of an individual {singularLeftName}.");
        SetParameterDescription(operation.Parameters[0], $"The identifier of the {singularLeftName} to add {rightName} to.");
        SetRequestBodyDescription(operation.RequestBody, $"The identities of the {rightName} to add to the {relationship} relationship.");

        SetResponseDescription(operation.Responses, HttpStatusCode.NoContent,
            $"The {rightName} were successfully added, which did not result in additional changes.");

        SetResponseDescription(operation.Responses, HttpStatusCode.BadRequest, TextRequestBodyBad);
        SetResponseDescription(operation.Responses, HttpStatusCode.NotFound, $"The {singularLeftName} or a related resource does not exist.");
        SetResponseDescription(operation.Responses, HttpStatusCode.Conflict, TextConflict);
    }

    private void ApplyPatchRelationship(OpenApiOperation operation, RelationshipAttribute relationship)
    {
        bool isOptional = _resourceFieldValidationMetadataProvider.IsNullable(relationship);
        string singularLeftName = relationship.LeftType.PublicName.Singularize();
        string rightName = relationship is HasOneAttribute ? relationship.RightType.PublicName.Singularize() : relationship.RightType.PublicName;

        SetOperationSummary(operation,
            relationship is HasOneAttribute
                ? isOptional
                    ? $"Clears or assigns an existing {rightName} to the {relationship} relationship of an individual {singularLeftName}."
                    : $"Assigns an existing {rightName} to the {relationship} relationship of an individual {singularLeftName}."
                : $"Assigns existing {rightName} to the {relationship} relationship of an individual {singularLeftName}.");

        SetParameterDescription(operation.Parameters[0],
            isOptional
                ? $"The identifier of the {singularLeftName} whose {relationship} relationship to assign or clear."
                : $"The identifier of the {singularLeftName} whose {relationship} relationship to assign.");

        SetRequestBodyDescription(operation.RequestBody,
            relationship is HasOneAttribute
                ? isOptional
                    ? $"The identity of the {rightName} to assign to the {relationship} relationship, or <c>null</c> to clear the relationship."
                    : $"The identity of the {rightName} to assign to the {relationship} relationship."
                : $"The identities of the {rightName} to assign to the {relationship} relationship, or an empty array to clear the relationship.");

        SetResponseDescription(operation.Responses, HttpStatusCode.NoContent,
            $"The {relationship} relationship was successfully updated, which did not result in additional changes.");

        SetResponseDescription(operation.Responses, HttpStatusCode.BadRequest, TextRequestBodyBad);
        SetResponseDescription(operation.Responses, HttpStatusCode.NotFound, $"The {singularLeftName} or a related resource does not exist.");
        SetResponseDescription(operation.Responses, HttpStatusCode.Conflict, TextConflict);
    }

    private void ApplyDeleteRelationship(OpenApiOperation operation, RelationshipAttribute relationship)
    {
        string singularLeftName = relationship.LeftType.PublicName.Singularize();
        string rightName = relationship.RightType.PublicName;

        SetOperationSummary(operation, $"Removes existing {rightName} from the {relationship} relationship of an individual {singularLeftName}.");
        SetParameterDescription(operation.Parameters[0], $"The identifier of the {singularLeftName} to remove {rightName} from.");
        SetRequestBodyDescription(operation.RequestBody, $"The identities of the {rightName} to remove from the {relationship} relationship.");

        SetResponseDescription(operation.Responses, HttpStatusCode.NoContent,
            $"The {rightName} were successfully removed, which did not result in additional changes.");

        SetResponseDescription(operation.Responses, HttpStatusCode.BadRequest, TextRequestBodyBad);
        SetResponseDescription(operation.Responses, HttpStatusCode.NotFound, $"The {singularLeftName} or a related resource does not exist.");
        SetResponseDescription(operation.Responses, HttpStatusCode.Conflict, TextConflict);
    }

    private static RelationshipAttribute GetRelationshipFromRoute(ApiDescription apiDescription, ResourceType resourceType)
    {
        if (apiDescription.RelativePath == null)
        {
            throw new UnreachableCodeException();
        }

        string relationshipName = apiDescription.RelativePath.Split('/').Last();
        return resourceType.GetRelationshipByPublicName(relationshipName);
    }

    private static void SetOperationSummary(OpenApiOperation operation, string description)
    {
        operation.Summary = XmlCommentsTextHelper.Humanize(description);
    }

    private static void SetOperationRemarks(OpenApiOperation operation, string description)
    {
        operation.Description = XmlCommentsTextHelper.Humanize(description);
    }

    private static void SetParameterDescription(OpenApiParameter parameter, string description)
    {
        parameter.Description = XmlCommentsTextHelper.Humanize(description);
    }

    private static void SetRequestBodyDescription(OpenApiRequestBody requestBody, string description)
    {
        requestBody.Description = XmlCommentsTextHelper.Humanize(description);
    }

    private static void SetResponseDescription(OpenApiResponses responses, HttpStatusCode statusCode, string description)
    {
        OpenApiResponse response = GetOrAddResponse(responses, statusCode);
        response.Description = XmlCommentsTextHelper.Humanize(description);
    }

    private static void SetResponseHeaderETag(OpenApiResponses responses, HttpStatusCode statusCode)
    {
        OpenApiResponse response = GetOrAddResponse(responses, statusCode);

        response.Headers[HeaderNames.ETag] = new OpenApiHeader
        {
            Description = "A fingerprint of the HTTP response, which can be used in an If-None-Match header to only fetch changes.",
            Required = true,
            Schema = new OpenApiSchema
            {
                Type = "string"
            }
        };
    }

    private static void SetResponseHeaderContentLength(OpenApiResponses responses, HttpStatusCode statusCode)
    {
        OpenApiResponse response = GetOrAddResponse(responses, statusCode);

        response.Headers[HeaderNames.ContentLength] = new OpenApiHeader
        {
            Description = "Size of the HTTP response body, in bytes.",
            Required = true,
            Schema = new OpenApiSchema
            {
                Type = "integer",
                Format = "int64"
            }
        };
    }

    private static void SetResponseHeaderLocation(OpenApiResponses responses, HttpStatusCode statusCode, string resourceName)
    {
        OpenApiResponse response = GetOrAddResponse(responses, statusCode);

        response.Headers[HeaderNames.Location] = new OpenApiHeader
        {
            Description = $"The URL at which the newly created {resourceName} can be retrieved.",
            Required = true,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Format = "uri"
            }
        };
    }

    private static OpenApiResponse GetOrAddResponse(OpenApiResponses responses, HttpStatusCode statusCode)
    {
        string responseCode = ((int)statusCode).ToString();

        if (!responses.TryGetValue(responseCode, out OpenApiResponse? response))
        {
            response = new OpenApiResponse();
            responses.Add(responseCode, response);
        }

        return response;
    }

    private static void AddQueryStringParameters(OpenApiOperation operation, bool isRelationshipEndpoint)
    {
        // The JSON:API query string parameters (include, filter, sort, page[size], page[number], fields[]) are too dynamic to represent in OpenAPI.
        // - The parameter names for fields[] require exploding to all resource types, because outcome of possible resource types depends on
        //     the relationship chains in include, which are provided at invocation time.
        // - The parameter names for filter/sort take a relationship path, which could be infinite. For example: ?filter[node.parent.parent.parent...]=...

        // The next best thing is to expose the query string parameters as unstructured and optional.
        // - This makes SwaggerUI ask for JSON, which is a bit odd, but it works. For example: {"sort":"-id"} produces: ?sort=-id
        // - This makes NSwag produce a C# client with method signature: GetAsync(IDictionary<string, string?>? query)
        //     when combined with <Options>/GenerateNullableReferenceTypes:true</Options> in the project file.

        operation.Parameters.Add(new OpenApiParameter
        {
            In = ParameterLocation.Query,
            Name = "query",
            Schema = new OpenApiSchema
            {
                Type = "object",
                AdditionalProperties = new OpenApiSchema
                {
                    Type = "string",
                    Nullable = true
                },
                // Prevent SwaggerUI from producing sample, which fails when used because unknown query string parameters are blocked by default.
                Example = new OpenApiString(string.Empty)
            },
            Description = isRelationshipEndpoint ? RelationshipQueryStringParameters : ResourceQueryStringParameters
        });
    }

    private static void AddRequestHeaderIfNoneMatch(OpenApiOperation operation)
    {
        operation.Parameters.Add(new OpenApiParameter
        {
            In = ParameterLocation.Header,
            Name = "If-None-Match",
            Description = "A list of ETags, resulting in HTTP status 304 without a body, if one of them matches the current fingerprint.",
            Schema = new OpenApiSchema
            {
                Type = "string"
            }
        });
    }

    private static void ApplyPostOperations(OpenApiOperation operation)
    {
        SetOperationSummary(operation, "Performs multiple mutations in a linear and atomic manner.");

        SetRequestBodyDescription(operation.RequestBody,
            "An array of mutation operations. For syntax, see the [Atomic Operations documentation](https://jsonapi.org/ext/atomic/).");

        SetResponseDescription(operation.Responses, HttpStatusCode.OK, "All operations were successfully applied, which resulted in additional changes.");

        SetResponseDescription(operation.Responses, HttpStatusCode.NoContent,
            "All operations were successfully applied, which did not result in additional changes.");

        SetResponseDescription(operation.Responses, HttpStatusCode.BadRequest, TextRequestBodyBad);
        SetResponseDescription(operation.Responses, HttpStatusCode.Forbidden, "An operation is not accessible or a client-generated ID is used.");
        SetResponseDescription(operation.Responses, HttpStatusCode.NotFound, "A resource or a related resource does not exist.");
        SetResponseDescription(operation.Responses, HttpStatusCode.Conflict, TextConflict);
        SetResponseDescription(operation.Responses, HttpStatusCode.UnprocessableEntity, TextRequestBodyValidationFailed);
    }
}
