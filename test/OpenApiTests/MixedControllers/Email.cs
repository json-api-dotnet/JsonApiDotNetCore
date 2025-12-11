using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace OpenApiTests.MixedControllers;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed record Email
{
    /// <summary>
    /// The email subject.
    /// </summary>
    [MaxLength(255)]
    public required string Subject { get; set; }

    /// <summary>
    /// The email body.
    /// </summary>
    public required string Body { get; set; }

    /// <summary>
    /// The email address of the sender.
    /// </summary>
    [EmailAddress]
    public required string From { get; set; }

    /// <summary>
    /// The email address of the recipient.
    /// </summary>
    [EmailAddress]
    public required string To { get; set; }

    /// <summary>
    /// The date/time (in UTC) at which this email was sent.
    /// </summary>
    public DateTimeOffset SentAtUtc { get; private set; }

    public void SetSentAt(DateTimeOffset utcValue)
    {
        SentAtUtc = utcValue;
    }
}
