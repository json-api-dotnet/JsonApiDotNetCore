# Microservices

The code [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests/Microservices) shows patterns commonly used in microservices architecture:

- [Fire-and-forget](https://microservices.io/patterns/communication-style/messaging.html): Outgoing messages are sent to an external queue, without waiting for their processing to start. While this is the simplest solution, it is not very reliable when errors occur.
- [Transactional Outbox Pattern](https://microservices.io/patterns/data/transactional-outbox.html): Outgoing messages are saved to a queue table within the same database transaction. A background job (omitted in this example) polls the queue table and sends the messages to an external queue.

> [!TIP]
> Potential external queue systems you could use are [RabbitMQ](https://www.rabbitmq.com/), [MassTransit](https://masstransit.io/),
> [Azure Service Bus](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview) and [Apache Kafka](https://kafka.apache.org/). However, this is beyond the scope of this topic.

The `Messages` directory lists the functional messages that are created from incoming JSON:API requests, which are typically processed by an external system that handles messages from the queue.
Each message has a unique ID and type, and is versioned to support gradual deployments.
Example payloads of messages are: user created, user login name changed, user moved to group, group created, group renamed, etc.

The abstract types `MessagingGroupDefinition` and `MessagingUserDefinition` are resource definitions that contain code shared by both patterns. They inspect the incoming request and produce one or more functional messages from it.
The pattern-specific derived types inject their `DbContext`, which is used to query for additional information when determining what is being changed.

> [!NOTE]
> Because networks are inherently unreliable, systems that consume messages from an external queue should be [idempotent](https://microservices.io/patterns/communication-style/idempotent-consumer.html).
> Several years ago, a [prototype](https://github.com/json-api-dotnet/JsonApiDotNetCore/pull/1132) was built to make JSON:API idempotent, but it was never finished due to a lack of community interest.
> Please [open an issue](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/new?labels=enhancement) if idempotency matters to you.
