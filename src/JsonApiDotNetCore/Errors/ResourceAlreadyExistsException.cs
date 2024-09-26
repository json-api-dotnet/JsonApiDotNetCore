using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors;

/// <summary>
/// The error that is thrown when creating a resource with an ID that already exists.
/// </summary>
[PublicAPI]
public sealed class ResourceAlreadyExistsException(string resourceId, string resourceType)
    : JsonApiException(new ErrorObject(HttpStatusCode.Conflict)
    {
        Title = "Another resource with the specified ID already exists.",
        Detail = $"Another resource of type '{resourceType}' with ID '{resourceId}' already exists."
    });
