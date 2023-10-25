using System.Net;
using Humanizer;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class JsonApiOperationDocumentationFilter : IOperationFilter
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

    private const string TextCompareETag =
        "Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.";

    private const string TextCompletedSuccessfully = "The operation completed successfully.";
    private const string TextRequestBodyMissingOrMalformed = "The request body is missing or malformed.";
    private const string TextRequestBodyIncompatibleType = "A resource type in the request body is incompatible.";
    private const string TextRequestBodyIncompatibleIdOrType = "A resource type or identifier in the request body is incompatible.";
    private const string TextRequestBodyValidationFailed = "Validation of the request body failed.";

    private readonly IControllerResourceMapping _controllerResourceMapping;
    private readonly ResourceFieldValidationMetadataProvider _resourceFieldValidationMetadataProvider;

    public JsonApiOperationDocumentationFilter(IControllerResourceMapping controllerResourceMapping,
        ResourceFieldValidationMetadataProvider resourceFieldValidationMetadataProvider)
    {
        ArgumentGuard.NotNull(controllerResourceMapping);
        ArgumentGuard.NotNull(resourceFieldValidationMetadataProvider);

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

        ResourceType? resourceType =
            _controllerResourceMapping.GetResourceTypeForController(context.ApiDescription.ActionDescriptor.GetActionMethod().ReflectedType);

        if (resourceType != null)
        {
            string actionName = context.MethodInfo.Name;

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
            }
            else
            {
                SetOperationSummary(operation, $"Retrieves a collection of {resourceType}.");

                SetResponseDescription(operation.Responses, HttpStatusCode.OK,
                    $"Successfully returns the found {resourceType}, or an empty array if none were found.");
            }
        }
        else if (operation.Parameters.Count == 1)
        {
            string singularName = resourceType.PublicName.Singularize();

            if (hasHeadVerb)
            {
                SetOperationSummary(operation, $"Retrieves an individual {singularName} by its identifier without returning it.");
                SetOperationRemarks(operation, TextCompareETag);
                SetResponseDescription(operation.Responses, HttpStatusCode.OK, TextCompletedSuccessfully);
            }
            else
            {
                SetOperationSummary(operation, $"Retrieves an individual {singularName} by its identifier.");
                SetResponseDescription(operation.Responses, HttpStatusCode.OK, $"Successfully returns the found {singularName}.");
            }

            SetParameterDescription(operation.Parameters[0], $"The identifier of the {singularName} to retrieve.");
            SetResponseDescription(operation.Responses, HttpStatusCode.NotFound, $"The {singularName} does not exist.");
        }
    }

    private void ApplyPostResource(OpenApiOperation operation, ResourceType resourceType)
    {
        string singularName = resourceType.PublicName.Singularize();

        SetOperationSummary(operation, $"Creates a new {singularName}.");
        SetRequestBodyDescription(operation.RequestBody, $"The attributes and relationships of the {singularName} to create.");

        SetResponseDescription(operation.Responses, HttpStatusCode.Created,
            $"The {singularName} was successfully created, which resulted in additional changes. The newly created {singularName} is returned.");

        SetResponseDescription(operation.Responses, HttpStatusCode.NoContent,
            $"The {singularName} was successfully created, which did not result in additional changes.");

        SetResponseDescription(operation.Responses, HttpStatusCode.BadRequest, TextRequestBodyMissingOrMalformed);
        SetResponseDescription(operation.Responses, HttpStatusCode.Conflict, TextRequestBodyIncompatibleType);
        SetResponseDescription(operation.Responses, HttpStatusCode.UnprocessableEntity, TextRequestBodyValidationFailed);
    }

    private void ApplyPatchResource(OpenApiOperation operation, ResourceType resourceType)
    {
        string singularName = resourceType.PublicName.Singularize();

        SetOperationSummary(operation, $"Updates an existing {singularName}.");
        SetParameterDescription(operation.Parameters[0], $"The identifier of the {singularName} to update.");

        SetRequestBodyDescription(operation.RequestBody,
            $"The attributes and relationships of the {singularName} to update. Omitted fields are left unchanged.");

        SetResponseDescription(operation.Responses, HttpStatusCode.OK,
            $"The {singularName} was successfully updated, which resulted in additional changes. The updated {singularName} is returned.");

        SetResponseDescription(operation.Responses, HttpStatusCode.NoContent,
            $"The {singularName} was successfully updated, which did not result in additional changes.");

        SetResponseDescription(operation.Responses, HttpStatusCode.NotFound, $"The {singularName} or a related resource does not exist.");
        SetResponseDescription(operation.Responses, HttpStatusCode.BadRequest, TextRequestBodyMissingOrMalformed);
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
        }
        else
        {
            SetOperationSummary(operation, $"Retrieves the related {rightName} of an individual {singularLeftName}'s {relationship} relationship.");

            SetResponseDescription(operation.Responses, HttpStatusCode.OK,
                relationship is HasOneAttribute
                    ? $"Successfully returns the found {rightName}, or <c>null</c> if it was not found."
                    : $"Successfully returns the found {rightName}, or an empty array if none were found.");
        }

        SetParameterDescription(operation.Parameters[0], $"The identifier of the {singularLeftName} whose related {rightName} to retrieve.");
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
        }
        else
        {
            SetOperationSummary(operation,
                $"Retrieves the related {singularRightName} {ident} of an individual {singularLeftName}'s {relationship} relationship.");

            SetResponseDescription(operation.Responses, HttpStatusCode.OK,
                relationship is HasOneAttribute
                    ? $"Successfully returns the found {singularRightName} {ident}, or <c>null</c> if it was not found."
                    : $"Successfully returns the found {singularRightName} {ident}, or an empty array if none were found.");
        }

        SetParameterDescription(operation.Parameters[0], $"The identifier of the {singularLeftName} whose related {singularRightName} {ident} to retrieve.");
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

        SetResponseDescription(operation.Responses, HttpStatusCode.NotFound, $"The {singularLeftName} does not exist.");
        SetResponseDescription(operation.Responses, HttpStatusCode.BadRequest, TextRequestBodyMissingOrMalformed);
        SetResponseDescription(operation.Responses, HttpStatusCode.Conflict, TextRequestBodyIncompatibleType);
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

        SetResponseDescription(operation.Responses, HttpStatusCode.NotFound, $"The {singularLeftName} does not exist.");
        SetResponseDescription(operation.Responses, HttpStatusCode.BadRequest, TextRequestBodyMissingOrMalformed);
        SetResponseDescription(operation.Responses, HttpStatusCode.Conflict, TextRequestBodyIncompatibleType);
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

        SetResponseDescription(operation.Responses, HttpStatusCode.NotFound, $"The {singularLeftName} does not exist.");
        SetResponseDescription(operation.Responses, HttpStatusCode.BadRequest, TextRequestBodyMissingOrMalformed);
        SetResponseDescription(operation.Responses, HttpStatusCode.Conflict, TextRequestBodyIncompatibleType);
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
        string responseCode = ((int)statusCode).ToString();

        if (!responses.TryGetValue(responseCode, out OpenApiResponse? response))
        {
            response = new OpenApiResponse();
            responses.Add(responseCode, response);
        }

        response.Description = XmlCommentsTextHelper.Humanize(description);
    }
}
