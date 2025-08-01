using System.Collections.Concurrent;

namespace OpenApiTests.MixedControllers;

public sealed class InMemoryOutgoingEmailsProvider
{
    public ConcurrentDictionary<DateTimeOffset, Email> SentEmails { get; } = new();
}
