using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace JsonApiDotNetCore.Middleware;

/// <inheritdoc />
[PublicAPI]
public class JsonApiContentNegotiator : IJsonApiContentNegotiator
{
    private readonly IJsonApiOptions _options;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private HttpContext HttpContext
    {
        get
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                throw new InvalidOperationException("An active HTTP request is required.");
            }

            return _httpContextAccessor.HttpContext;
        }
    }

    public JsonApiContentNegotiator(IJsonApiOptions options, IHttpContextAccessor httpContextAccessor)
    {
        ArgumentGuard.NotNull(options);
        ArgumentGuard.NotNull(httpContextAccessor);

        _options = options;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public IReadOnlySet<JsonApiMediaTypeExtension> Negotiate()
    {
        IReadOnlyList<JsonApiMediaType> possibleMediaTypes = GetPossibleMediaTypes();

        JsonApiMediaType? requestMediaType = ValidateContentType(possibleMediaTypes);
        return ValidateAcceptHeader(possibleMediaTypes, requestMediaType);
    }

    private JsonApiMediaType? ValidateContentType(IReadOnlyList<JsonApiMediaType> possibleMediaTypes)
    {
        if (HttpContext.Request.ContentType == null)
        {
            if (HttpContext.Request.ContentLength > 0)
            {
                throw CreateContentTypeError(possibleMediaTypes);
            }

            return null;
        }

        JsonApiMediaType? mediaType = JsonApiMediaType.TryParseContentTypeHeaderValue(HttpContext.Request.ContentType);

        if (mediaType == null || !possibleMediaTypes.Contains(mediaType))
        {
            throw CreateContentTypeError(possibleMediaTypes);
        }

        return mediaType;
    }

    private IReadOnlySet<JsonApiMediaTypeExtension> ValidateAcceptHeader(IReadOnlyList<JsonApiMediaType> possibleMediaTypes, JsonApiMediaType? requestMediaType)
    {
        string[] acceptHeaderValues = HttpContext.Request.Headers.GetCommaSeparatedValues("Accept");
        JsonApiMediaType? bestMatch = null;

        if (acceptHeaderValues.Length == 0)
        {
            bestMatch = GetDefaultMediaType(possibleMediaTypes, requestMediaType);
        }
        else
        {
            decimal bestQualityFactor = 0m;

            foreach (string acceptHeaderValue in acceptHeaderValues)
            {
                (JsonApiMediaType MediaType, decimal QualityFactor)? result = JsonApiMediaType.TryParseAcceptHeaderValue(acceptHeaderValue);

                if (result != null)
                {
                    if (result.Value.MediaType.Equals(requestMediaType) && possibleMediaTypes.Contains(requestMediaType))
                    {
                        // Content-Type always wins over other candidates, because JsonApiDotNetCore doesn't support
                        // different extension sets for the request and response body.
                        bestMatch = requestMediaType;
                        break;
                    }

                    bool isBetterMatch = false;
                    int? currentIndex = null;

                    if (bestMatch == null)
                    {
                        isBetterMatch = true;
                    }
                    else if (result.Value.QualityFactor > bestQualityFactor)
                    {
                        isBetterMatch = true;
                    }
                    else if (result.Value.QualityFactor == bestQualityFactor)
                    {
                        if (result.Value.MediaType.Extensions.Count > bestMatch.Extensions.Count)
                        {
                            isBetterMatch = true;
                        }
                        else if (result.Value.MediaType.Extensions.Count == bestMatch.Extensions.Count)
                        {
                            int bestIndex = possibleMediaTypes.FindIndex(bestMatch);
                            currentIndex = possibleMediaTypes.FindIndex(result.Value.MediaType);

                            if (currentIndex != -1 && currentIndex < bestIndex)
                            {
                                isBetterMatch = true;
                            }
                        }
                    }

                    if (isBetterMatch)
                    {
                        bool existsInPossibleMediaTypes = currentIndex >= 0 || possibleMediaTypes.Contains(result.Value.MediaType);

                        if (existsInPossibleMediaTypes)
                        {
                            bestMatch = result.Value.MediaType;
                            bestQualityFactor = result.Value.QualityFactor;
                        }
                    }
                }
            }
        }

        if (bestMatch == null)
        {
            throw CreateAcceptHeaderError(possibleMediaTypes);
        }

        if (requestMediaType != null && !bestMatch.Equals(requestMediaType))
        {
            throw CreateAcceptHeaderError(possibleMediaTypes);
        }

        return bestMatch.Extensions;
    }

    /// <summary>
    /// Returns the JSON:API media type (possibly including extensions) to use when no Accept header was sent.
    /// </summary>
    /// <param name="possibleMediaTypes">
    /// The media types returned from <see cref="GetPossibleMediaTypes" />.
    /// </param>
    /// <param name="requestMediaType">
    /// The media type from in the Content-Type header.
    /// </param>
    /// <returns>
    /// The default media type to use, or <c>null</c> if not available.
    /// </returns>
    protected virtual JsonApiMediaType? GetDefaultMediaType(IReadOnlyList<JsonApiMediaType> possibleMediaTypes, JsonApiMediaType? requestMediaType)
    {
        return possibleMediaTypes.Contains(JsonApiMediaType.Default) ? JsonApiMediaType.Default : null;
    }

    /// <summary>
    /// Gets the list of possible combinations of JSON:API extensions that are available at the current endpoint. The set of extensions in the request body
    /// must always be the same as in the response body.
    /// </summary>
    /// <remarks>
    /// Override this method to add support for custom JSON:API extensions. Implementations should take <see cref="IJsonApiOptions.Extensions" /> into
    /// account. During content negotiation, the first compatible entry with the highest number of extensions is preferred, but beware that clients can
    /// overrule this using quality factors in an Accept header.
    /// </remarks>
    protected virtual IReadOnlyList<JsonApiMediaType> GetPossibleMediaTypes()
    {
        List<JsonApiMediaType> mediaTypes = [];

        // Relaxed entries come after JSON:API compliant entries, which makes them less likely to be selected.

        if (IsOperationsEndpoint())
        {
            if (_options.Extensions.Contains(JsonApiMediaTypeExtension.AtomicOperations))
            {
                mediaTypes.Add(JsonApiMediaType.AtomicOperations);
            }

            if (_options.Extensions.Contains(JsonApiMediaTypeExtension.RelaxedAtomicOperations))
            {
                mediaTypes.Add(JsonApiMediaType.RelaxedAtomicOperations);
            }
        }
        else
        {
            mediaTypes.Add(JsonApiMediaType.Default);
        }

        return mediaTypes.AsReadOnly();
    }

    protected bool IsOperationsEndpoint()
    {
        RouteValueDictionary routeValues = HttpContext.GetRouteData().Values;
        return JsonApiMiddleware.IsRouteForOperations(routeValues);
    }

    private JsonApiException CreateContentTypeError(IReadOnlyList<JsonApiMediaType> possibleMediaTypes)
    {
        string allowedValues = string.Join(" or ", possibleMediaTypes.Select(mediaType => $"'{mediaType}'"));

        return new JsonApiException(new ErrorObject(HttpStatusCode.UnsupportedMediaType)
        {
            Title = "The specified Content-Type header value is not supported.",
            Detail = $"Use {allowedValues} instead of '{HttpContext.Request.ContentType}' for the Content-Type header value.",
            Source = new ErrorSource
            {
                Header = "Content-Type"
            }
        });
    }

    private static JsonApiException CreateAcceptHeaderError(IReadOnlyList<JsonApiMediaType> possibleMediaTypes)
    {
        string allowedValues = string.Join(" or ", possibleMediaTypes.Select(mediaType => $"'{mediaType}'"));

        return new JsonApiException(new ErrorObject(HttpStatusCode.NotAcceptable)
        {
            Title = "The specified Accept header value does not contain any supported media types.",
            Detail = $"Include {allowedValues} in the Accept header values.",
            Source = new ErrorSource
            {
                Header = "Accept"
            }
        });
    }
}
