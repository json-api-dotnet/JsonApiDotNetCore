using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.IntegrationTests.Microservices.Messages;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Microservices.FireAndForgetDelivery
{
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
                throw new JsonApiException(new Error(HttpStatusCode.ServiceUnavailable)
                {
                    Title = "Message delivery failed."
                });
            }

            return Task.CompletedTask;
        }
    }
}
