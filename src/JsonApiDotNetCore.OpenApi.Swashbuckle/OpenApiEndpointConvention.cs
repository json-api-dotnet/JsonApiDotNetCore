using System.Diagnostics;
using System.Net;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata;
using JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.Documents;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

/// <summary>
/// Sets metadata on controllers for OpenAPI documentation generation by Swagger. Only targets JsonApiDotNetCore controllers.
/// </summary>
internal sealed class OpenApiEndpointConvention : IActionModelConvention
{
    private readonly IControllerResourceMapping _controllerResourceMapping;
    private readonly IJsonApiOptions _options;

    public OpenApiEndpointConvention(IControllerResourceMapping controllerResourceMapping, IJsonApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(controllerResourceMapping);
        ArgumentNullException.ThrowIfNull(options);

        _controllerResourceMapping = controllerResourceMapping;
        _options = options;
    }

    public void Apply(ActionModel action)
    {
        ArgumentNullException.ThrowIfNull(action);

        JsonApiEndpointWrapper endpoint = JsonApiEndpointWrapper.FromActionModel(action);

        if (endpoint.IsUnknown)
        {
            // Not a JSON:API controller, or a non-standard action method in a JSON:API controller.
            // None of these are yet implemented, so hide them to avoid downstream crashes.
            action.ApiExplorer.IsVisible = false;
            return;
        }

        ResourceType? resourceType = _controllerResourceMapping.GetResourceTypeForController(action.Controller.ControllerType);

        if (ShouldSuppressEndpoint(endpoint, resourceType))
        {
            action.ApiExplorer.IsVisible = false;
            return;
        }

        SetResponseMetadata(action, endpoint, resourceType);
        SetRequestMetadata(action, endpoint);
    }

    private bool ShouldSuppressEndpoint(JsonApiEndpointWrapper endpoint, ResourceType? resourceType)
    {
        if (resourceType == null)
        {
            return false;
        }

        if (!IsEndpointAvailable(endpoint.Value, resourceType))
        {
            return true;
        }

        if (IsSecondaryOrRelationshipEndpoint(endpoint.Value))
        {
            if (resourceType.Relationships.Count == 0)
            {
                return true;
            }

            if (endpoint.Value is JsonApiEndpoints.DeleteRelationship or JsonApiEndpoints.PostRelationship)
            {
                return !resourceType.Relationships.OfType<HasManyAttribute>().Any();
            }
        }

        return false;
    }

    private static bool IsEndpointAvailable(JsonApiEndpoints endpoint, ResourceType resourceType)
    {
        JsonApiEndpoints availableEndpoints = GetGeneratedControllerEndpoints(resourceType);

        if (availableEndpoints == JsonApiEndpoints.None)
        {
            // Auto-generated controllers are disabled, so we can't know what to hide.
            // It is assumed that a handwritten JSON:API controller only provides action methods for what it supports.
            // To accomplish that, derive from BaseJsonApiController instead of JsonApiController.
            return true;
        }

        // For an overridden JSON:API action method in a partial class to show up, it's flag must be turned on in [Resource].
        // Otherwise, it is considered to be an action method that throws because the endpoint is unavailable.
        return endpoint switch
        {
            JsonApiEndpoints.GetCollection => availableEndpoints.HasFlag(JsonApiEndpoints.GetCollection),
            JsonApiEndpoints.GetSingle => availableEndpoints.HasFlag(JsonApiEndpoints.GetSingle),
            JsonApiEndpoints.GetSecondary => availableEndpoints.HasFlag(JsonApiEndpoints.GetSecondary),
            JsonApiEndpoints.GetRelationship => availableEndpoints.HasFlag(JsonApiEndpoints.GetRelationship),
            JsonApiEndpoints.Post => availableEndpoints.HasFlag(JsonApiEndpoints.Post),
            JsonApiEndpoints.PostRelationship => availableEndpoints.HasFlag(JsonApiEndpoints.PostRelationship),
            JsonApiEndpoints.Patch => availableEndpoints.HasFlag(JsonApiEndpoints.Patch),
            JsonApiEndpoints.PatchRelationship => availableEndpoints.HasFlag(JsonApiEndpoints.PatchRelationship),
            JsonApiEndpoints.Delete => availableEndpoints.HasFlag(JsonApiEndpoints.Delete),
            JsonApiEndpoints.DeleteRelationship => availableEndpoints.HasFlag(JsonApiEndpoints.DeleteRelationship),
            _ => throw new UnreachableException()
        };
    }

    private static JsonApiEndpoints GetGeneratedControllerEndpoints(ResourceType resourceType)
    {
        var resourceAttribute = resourceType.ClrType.GetCustomAttribute<ResourceAttribute>();
        return resourceAttribute?.GenerateControllerEndpoints ?? JsonApiEndpoints.None;
    }

    private static bool IsSecondaryOrRelationshipEndpoint(JsonApiEndpoints endpoint)
    {
        return endpoint is JsonApiEndpoints.GetSecondary or JsonApiEndpoints.GetRelationship or JsonApiEndpoints.PostRelationship or
            JsonApiEndpoints.PatchRelationship or JsonApiEndpoints.DeleteRelationship;
    }

    private void SetResponseMetadata(ActionModel action, JsonApiEndpointWrapper endpoint, ResourceType? resourceType)
    {
        JsonApiMediaType mediaType = GetMediaTypeForEndpoint(endpoint);
        action.Filters.Add(new ProducesAttribute(mediaType.ToString()));

        foreach (HttpStatusCode statusCode in GetSuccessStatusCodesForEndpoint(endpoint))
        {
            // The return type is set later by JsonApiActionDescriptorCollectionProvider.
            action.Filters.Add(new ProducesResponseTypeAttribute((int)statusCode));
        }

        foreach (HttpStatusCode statusCode in GetErrorStatusCodesForEndpoint(endpoint, resourceType))
        {
            action.Filters.Add(new ProducesResponseTypeAttribute(typeof(ErrorResponseDocument), (int)statusCode));
        }
    }

    private JsonApiMediaType GetMediaTypeForEndpoint(JsonApiEndpointWrapper endpoint)
    {
        return endpoint.IsAtomicOperationsEndpoint ? JsonApiMediaType.RelaxedAtomicOperations : JsonApiMediaType.Default;
    }

    private static HttpStatusCode[] GetSuccessStatusCodesForEndpoint(JsonApiEndpointWrapper endpoint)
    {
        if (endpoint.IsAtomicOperationsEndpoint)
        {
            return
            [
                HttpStatusCode.OK,
                HttpStatusCode.NoContent
            ];
        }

        return endpoint.Value switch
        {
            JsonApiEndpoints.GetCollection or JsonApiEndpoints.GetSingle or JsonApiEndpoints.GetSecondary or JsonApiEndpoints.GetRelationship =>
            [
                HttpStatusCode.OK,
                HttpStatusCode.NotModified
            ],
            JsonApiEndpoints.Post =>
            [
                HttpStatusCode.Created,
                HttpStatusCode.NoContent
            ],
            JsonApiEndpoints.Patch =>
            [
                HttpStatusCode.OK,
                HttpStatusCode.NoContent
            ],
            JsonApiEndpoints.Delete or JsonApiEndpoints.PostRelationship or JsonApiEndpoints.PatchRelationship or JsonApiEndpoints.DeleteRelationship =>
            [
                HttpStatusCode.NoContent
            ],
            _ => throw new UnreachableException()
        };
    }

    private HttpStatusCode[] GetErrorStatusCodesForEndpoint(JsonApiEndpointWrapper endpoint, ResourceType? resourceType)
    {
        if (endpoint.IsAtomicOperationsEndpoint)
        {
            return
            [
                HttpStatusCode.BadRequest,
                HttpStatusCode.Forbidden,
                HttpStatusCode.NotFound,
                HttpStatusCode.Conflict,
                HttpStatusCode.UnprocessableEntity
            ];
        }

        // Condition doesn't apply to atomic operations, because Forbidden is also used when an operation is not accessible.
        ClientIdGenerationMode clientIdGeneration = resourceType?.ClientIdGeneration ?? _options.ClientIdGeneration;

        return endpoint.Value switch
        {
            JsonApiEndpoints.GetCollection => [HttpStatusCode.BadRequest],
            JsonApiEndpoints.GetSingle or JsonApiEndpoints.GetSecondary or JsonApiEndpoints.GetRelationship =>
            [
                HttpStatusCode.BadRequest,
                HttpStatusCode.NotFound
            ],
            JsonApiEndpoints.Post when clientIdGeneration == ClientIdGenerationMode.Forbidden =>
            [
                HttpStatusCode.BadRequest,
                HttpStatusCode.Forbidden,
                HttpStatusCode.NotFound,
                HttpStatusCode.Conflict,
                HttpStatusCode.UnprocessableEntity
            ],
            JsonApiEndpoints.Post =>
            [
                HttpStatusCode.BadRequest,
                HttpStatusCode.NotFound,
                HttpStatusCode.Conflict,
                HttpStatusCode.UnprocessableEntity
            ],
            JsonApiEndpoints.Patch =>
            [
                HttpStatusCode.BadRequest,
                HttpStatusCode.NotFound,
                HttpStatusCode.Conflict,
                HttpStatusCode.UnprocessableEntity
            ],
            JsonApiEndpoints.Delete => [HttpStatusCode.NotFound],
            JsonApiEndpoints.PostRelationship or JsonApiEndpoints.PatchRelationship or JsonApiEndpoints.DeleteRelationship =>
            [
                HttpStatusCode.BadRequest,
                HttpStatusCode.NotFound,
                HttpStatusCode.Conflict
            ],
            _ => throw new UnreachableException()
        };
    }

    private void SetRequestMetadata(ActionModel action, JsonApiEndpointWrapper endpoint)
    {
        if (RequiresRequestBody(endpoint))
        {
            JsonApiMediaType mediaType = GetMediaTypeForEndpoint(endpoint);
            action.Filters.Add(new ConsumesAttribute(mediaType.ToString()));
        }
    }

    private static bool RequiresRequestBody(JsonApiEndpointWrapper endpoint)
    {
        return endpoint.IsAtomicOperationsEndpoint || endpoint.Value is JsonApiEndpoints.Post or JsonApiEndpoints.Patch or JsonApiEndpoints.PostRelationship or
            JsonApiEndpoints.PatchRelationship or JsonApiEndpoints.DeleteRelationship;
    }

    private sealed class JsonApiEndpointWrapper
    {
        private static readonly JsonApiEndpointWrapper AtomicOperations = new(true, JsonApiEndpoints.None);

        public bool IsAtomicOperationsEndpoint { get; }
        public JsonApiEndpoints Value { get; }
        public bool IsUnknown => !IsAtomicOperationsEndpoint && Value == JsonApiEndpoints.None;

        private JsonApiEndpointWrapper(bool isAtomicOperationsEndpoint, JsonApiEndpoints value)
        {
            IsAtomicOperationsEndpoint = isAtomicOperationsEndpoint;
            Value = value;
        }

        public static JsonApiEndpointWrapper FromActionModel(ActionModel actionModel)
        {
            if (EndpointResolver.Instance.IsAtomicOperationsController(actionModel.ActionMethod))
            {
                return AtomicOperations;
            }

            JsonApiEndpoints endpoint = EndpointResolver.Instance.GetEndpoint(actionModel.ActionMethod);
            return new JsonApiEndpointWrapper(false, endpoint);
        }

        public override string ToString()
        {
            return IsAtomicOperationsEndpoint ? "PostOperations" : Value.ToString();
        }
    }
}
