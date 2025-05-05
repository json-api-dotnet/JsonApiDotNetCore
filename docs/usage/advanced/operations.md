# Atomic Operations

The code [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests/AtomicOperations) covers usage of the [Atomic Operations](https://jsonapi.org/ext/atomic/) extension, which enables sending multiple changes in a single request.

- Operations for creating, updating, and deleting resources and relationships are shown.
- If one of the operations fails, the transaction is rolled back.
- Local IDs are used to reference resources created in a preceding operation within the same request.
- A custom controller restricts which operations are allowed, per resource type.
- The maximum number of operations per request can be configured at startup.
- For efficiency, operations are validated upfront (before accessing the database). If validation fails, the list of all errors is returned.
  - Takes [ASP.NET Model Validation](https://learn.microsoft.com/aspnet/web-api/overview/formats-and-model-binding/model-validation-in-aspnet-web-api) attributes into account.
  - See `DateMustBeInThePastAttribute` for how to implement a custom model validator.
- Various interactions with resource definitions are shown.

The Atomic Operations extension is enabled after an operations controller is added to the project. No further code is needed.
