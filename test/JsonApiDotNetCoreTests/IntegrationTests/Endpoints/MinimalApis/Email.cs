using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.MinimalApis;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed record Email
{
    [MaxLength(255)]
    public required string Subject { get; set; }

    public required string Body { get; set; }

    [EmailAddress]
    public required string From { get; set; }

    [EmailAddress]
    public required string To { get; set; }

    [JsonInclude]
    public DateTimeOffset SentAtUtc { get; private set; }

    public void SetSentAt(DateTimeOffset utcValue)
    {
        SentAtUtc = utcValue;
    }
}
