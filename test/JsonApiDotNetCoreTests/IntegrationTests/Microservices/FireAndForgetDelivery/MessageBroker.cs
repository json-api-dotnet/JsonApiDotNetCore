using System.Net;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.FireAndForgetDelivery;

public sealed class MessageBroker
{
    internal IList<OutgoingMessage> SentMessages { get; } = new List<OutgoingMessage>();

    internal bool SimulateFailure { get; set; }

    internal void Reset()
    {
        SimulateFailure = false;
        SentMessages.Clear();
    }

    internal Task PostMessageAsync(OutgoingMessage message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SentMessages.Add(message);

        if (SimulateFailure)
        {
            throw new JsonApiException(new ErrorObject(HttpStatusCode.ServiceUnavailable)
            {
                Title = "Message delivery failed."
            });
        }

        return Task.CompletedTask;
    }
}
