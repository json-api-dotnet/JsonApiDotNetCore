using System.Net;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization.Objects;

/// <summary>
/// See https://jsonapi.org/format/#error-objects.
/// </summary>
[PublicAPI]
public sealed class ErrorObject
{
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("links")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ErrorLinks? Links { get; set; }

    [JsonIgnore]
    public HttpStatusCode StatusCode { get; set; }

    [JsonPropertyName("status")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string Status
    {
        get => StatusCode.ToString("d");
        set => StatusCode = (HttpStatusCode)int.Parse(value);
    }

    [JsonPropertyName("code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Code { get; set; }

    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; set; }

    [JsonPropertyName("detail")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Detail { get; set; }

    [JsonPropertyName("source")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ErrorSource? Source { get; set; }

    [JsonPropertyName("meta")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, object?>? Meta { get; set; }

    public ErrorObject(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
    }

    public static HttpStatusCode GetResponseStatusCode(IReadOnlyList<ErrorObject> errorObjects)
    {
        if (errorObjects.IsNullOrEmpty())
        {
            return HttpStatusCode.InternalServerError;
        }

        int[] statusCodes = errorObjects.Select(error => (int)error.StatusCode).Distinct().ToArray();

        if (statusCodes.Length == 1)
        {
            return (HttpStatusCode)statusCodes[0];
        }

        int statusCode = int.Parse($"{statusCodes.Max().ToString()[0]}00");
        return (HttpStatusCode)statusCode;
    }
}
