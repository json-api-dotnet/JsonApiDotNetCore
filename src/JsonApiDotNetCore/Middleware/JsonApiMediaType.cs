using JetBrains.Annotations;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace JsonApiDotNetCore.Middleware;

/// <summary>
/// Represents the JSON:API media type (application/vnd.api+json) with an optional set of extensions.
/// </summary>
[PublicAPI]
public sealed class JsonApiMediaType : IEquatable<JsonApiMediaType>
{
    private static readonly StringSegment BaseMediaTypeSegment = new("application/vnd.api+json");
    private static readonly StringSegment ExtSegment = new("ext");
    private static readonly StringSegment QualitySegment = new("q");

    /// <summary>
    /// Gets the JSON:API media type without any extensions.
    /// </summary>
    public static readonly JsonApiMediaType Default = new([]);

    /// <summary>
    /// Gets the JSON:API media type with the "https://jsonapi.org/ext/atomic" extension.
    /// </summary>
    public static readonly JsonApiMediaType AtomicOperations = new([JsonApiMediaTypeExtension.AtomicOperations]);

    /// <summary>
    /// Gets the JSON:API media type with the "atomic" extension.
    /// </summary>
    public static readonly JsonApiMediaType RelaxedAtomicOperations = new([JsonApiMediaTypeExtension.RelaxedAtomicOperations]);

    public IReadOnlySet<JsonApiMediaTypeExtension> Extensions { get; }

    public JsonApiMediaType(IReadOnlySet<JsonApiMediaTypeExtension> extensions)
    {
        ArgumentNullException.ThrowIfNull(extensions);

        Extensions = extensions;
    }

    public JsonApiMediaType(IEnumerable<JsonApiMediaTypeExtension> extensions)
    {
        ArgumentNullException.ThrowIfNull(extensions);

        Extensions = extensions.ToHashSet().AsReadOnly();
    }

    internal static JsonApiMediaType? TryParseContentTypeHeaderValue(string value)
    {
        (JsonApiMediaType MediaType, decimal QualityFactor)? result = TryParse(value, false, false);
        return result?.MediaType;
    }

    internal static (JsonApiMediaType MediaType, decimal QualityFactor)? TryParseAcceptHeaderValue(string value)
    {
        return TryParse(value, true, true);
    }

    private static (JsonApiMediaType MediaType, decimal QualityFactor)? TryParse(string value, bool allowSuperset, bool allowQualityFactor)
    {
        // Parameter names are case-insensitive, according to https://datatracker.ietf.org/doc/html/rfc7231#section-3.1.1.1.
        // But JSON:API doesn't define case-insensitive for the "ext" parameter value.

        if (MediaTypeHeaderValue.TryParse(value, out MediaTypeHeaderValue? headerValue))
        {
            bool isBaseMatch = allowSuperset
                ? headerValue.MatchesMediaType(BaseMediaTypeSegment)
                : BaseMediaTypeSegment.Equals(headerValue.MediaType, StringComparison.OrdinalIgnoreCase);

            if (isBaseMatch)
            {
                HashSet<JsonApiMediaTypeExtension> extensions = [];

                decimal qualityFactor = 1.0m;

                foreach (NameValueHeaderValue parameter in headerValue.Parameters)
                {
                    if (allowQualityFactor && parameter.Name.Equals(QualitySegment, StringComparison.OrdinalIgnoreCase) &&
                        decimal.TryParse(parameter.Value, out decimal qualityValue))
                    {
                        qualityFactor = qualityValue;
                        continue;
                    }

                    if (!parameter.Name.Equals(ExtSegment, StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }

                    ParseExtensions(parameter, extensions);
                }

                return (new JsonApiMediaType(extensions), qualityFactor);
            }
        }

        return null;
    }

    private static void ParseExtensions(NameValueHeaderValue parameter, HashSet<JsonApiMediaTypeExtension> extensions)
    {
        string parameterValue = parameter.GetUnescapedValue().ToString();

        foreach (string extValue in parameterValue.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var extension = new JsonApiMediaTypeExtension(extValue);
            extensions.Add(extension);
        }
    }

    public override string ToString()
    {
        var baseHeaderValue = new MediaTypeHeaderValue(BaseMediaTypeSegment);
        List<NameValueHeaderValue> parameters = [];
        bool requiresEscape = false;

        foreach (JsonApiMediaTypeExtension extension in Extensions)
        {
            var extHeaderValue = new NameValueHeaderValue(ExtSegment);
            extHeaderValue.SetAndEscapeValue(extension.UnescapedValue);

            if (extHeaderValue.Value != extension.UnescapedValue)
            {
                requiresEscape = true;
            }

            parameters.Add(extHeaderValue);
        }

        if (parameters.Count == 1)
        {
            baseHeaderValue.Parameters.Add(parameters[0]);
        }
        else if (parameters.Count > 1)
        {
            if (requiresEscape)
            {
                // JSON:API requires all 'ext' parameters combined into a single space-separated value.
                string compositeValue = string.Join(' ', parameters.Select(parameter => parameter.GetUnescapedValue().ToString()));
                var compositeParameter = new NameValueHeaderValue(ExtSegment);
                compositeParameter.SetAndEscapeValue(compositeValue);
                baseHeaderValue.Parameters.Add(compositeParameter);
            }
            else
            {
                // Relaxed mode: use separate 'ext' parameters.
                foreach (NameValueHeaderValue parameter in parameters)
                {
                    baseHeaderValue.Parameters.Add(parameter);
                }
            }
        }

        return baseHeaderValue.ToString();
    }

    public bool Equals(JsonApiMediaType? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Extensions.SetEquals(other.Extensions);
    }

    public override bool Equals(object? other)
    {
        return Equals(other as JsonApiMediaType);
    }

    public override int GetHashCode()
    {
        int hashCode = 0;

        foreach (JsonApiMediaTypeExtension extension in Extensions)
        {
            hashCode = HashCode.Combine(hashCode, extension);
        }

        return hashCode;
    }
}
