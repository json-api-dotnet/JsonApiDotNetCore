using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.OpenApi.JsonApiMetadata;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace JsonApiDotNetCore.OpenApi;

/// <summary>
/// Sets metadata on controllers for OpenAPI documentation generation by Swagger. Only targets JsonApiDotNetCore controllers.
/// </summary>
internal sealed class OpenApiEndpointConvention : IActionModelConvention
{
    private readonly IControllerResourceMapping _controllerResourceMapping;
    private readonly EndpointResolver _endpointResolver = new();

    public OpenApiEndpointConvention(IControllerResourceMapping controllerResourceMapping)
    {
        ArgumentGuard.NotNull(controllerResourceMapping);

        _controllerResourceMapping = controllerResourceMapping;
    }

    public void Apply(ActionModel action)
    {
        ArgumentGuard.NotNull(action);

        JsonApiEndpoint? endpoint = _endpointResolver.Get(action.ActionMethod);

        if (endpoint == null || ShouldSuppressEndpoint(endpoint.Value, action.Controller.ControllerType))
        {
            action.ApiExplorer.IsVisible = false;

            return;
        }

        SetResponseMetadata(action, endpoint.Value);

        SetRequestMetadata(action, endpoint.Value);
    }

    private bool ShouldSuppressEndpoint(JsonApiEndpoint endpoint, Type controllerType)
    {
        if (IsSecondaryOrRelationshipEndpoint(endpoint))
        {
            IReadOnlyCollection<RelationshipAttribute> relationships = GetRelationshipsOfPrimaryResource(controllerType);

            if (!relationships.Any())
            {
                return true;
            }

            if (endpoint is JsonApiEndpoint.DeleteRelationship or JsonApiEndpoint.PostRelationship)
            {
                return !relationships.OfType<HasManyAttribute>().Any();
            }
        }

        return false;
    }

    private IReadOnlyCollection<RelationshipAttribute> GetRelationshipsOfPrimaryResource(Type controllerType)
    {
        ResourceType? primaryResourceType = _controllerResourceMapping.GetResourceTypeForController(controllerType);

        if (primaryResourceType == null)
        {
            throw new UnreachableCodeException();
        }

        return primaryResourceType.Relationships;
    }

    private static bool IsSecondaryOrRelationshipEndpoint(JsonApiEndpoint endpoint)
    {
        return endpoint is JsonApiEndpoint.GetSecondary or JsonApiEndpoint.GetRelationship or JsonApiEndpoint.PostRelationship or
            JsonApiEndpoint.PatchRelationship or JsonApiEndpoint.DeleteRelationship;
    }

    private void SetResponseMetadata(ActionModel action, JsonApiEndpoint endpoint)
    {
        IList<int> statusCodes = GetStatusCodesForEndpoint(endpoint);

        foreach (int statusCode in statusCodes)
        {
            action.Filters.Add(new ProducesResponseTypeAttribute(statusCode));

            switch (endpoint)
            {
                case JsonApiEndpoint.GetCollection when statusCode == StatusCodes.Status200OK:
                case JsonApiEndpoint.Post when statusCode == StatusCodes.Status201Created:
                case JsonApiEndpoint.Patch when statusCode == StatusCodes.Status200OK:
                case JsonApiEndpoint.GetSingle when statusCode == StatusCodes.Status200OK:
                case JsonApiEndpoint.GetSecondary when statusCode == StatusCodes.Status200OK:
                case JsonApiEndpoint.GetRelationship when statusCode == StatusCodes.Status200OK:
                {
                    action.Filters.Add(new ProducesAttribute(HeaderConstants.MediaType));
                    break;
                }
            }
        }
    }

    private static IList<int> GetStatusCodesForEndpoint(JsonApiEndpoint endpoint)
    {
        switch (endpoint)
        {
            case JsonApiEndpoint.GetCollection:
            case JsonApiEndpoint.GetSingle:
            case JsonApiEndpoint.GetSecondary:
            case JsonApiEndpoint.GetRelationship:
            {
                return new[]
                {
                    StatusCodes.Status200OK
                };
            }
            case JsonApiEndpoint.Post:
            {
                return new[]
                {
                    StatusCodes.Status201Created,
                    StatusCodes.Status204NoContent
                };
            }
            case JsonApiEndpoint.Patch:
            {
                return new[]
                {
                    StatusCodes.Status200OK,
                    StatusCodes.Status204NoContent
                };
            }
            case JsonApiEndpoint.Delete:
            case JsonApiEndpoint.PostRelationship:
            case JsonApiEndpoint.PatchRelationship:
            case JsonApiEndpoint.DeleteRelationship:
            {
                return new[]
                {
                    StatusCodes.Status204NoContent
                };
            }
            default:
            {
                throw new UnreachableCodeException();
            }
        }
    }

    private static void SetRequestMetadata(ActionModel action, JsonApiEndpoint endpoint)
    {
        if (RequiresRequestBody(endpoint))
        {
            action.Filters.Add(new ConsumesAttribute(HeaderConstants.MediaType));
        }
    }

    private static bool RequiresRequestBody(JsonApiEndpoint endpoint)
    {
        return endpoint is JsonApiEndpoint.Post or JsonApiEndpoint.Patch or JsonApiEndpoint.PostRelationship or JsonApiEndpoint.PatchRelationship or
            JsonApiEndpoint.DeleteRelationship;
    }
}
