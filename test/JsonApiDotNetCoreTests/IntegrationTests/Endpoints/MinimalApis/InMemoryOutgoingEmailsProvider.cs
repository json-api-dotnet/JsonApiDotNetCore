using System.Collections.Concurrent;

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.MinimalApis;

public sealed class InMemoryOutgoingEmailsProvider
{
    internal ConcurrentDictionary<DateTimeOffset, Email> SentEmails { get; } = new();
}
