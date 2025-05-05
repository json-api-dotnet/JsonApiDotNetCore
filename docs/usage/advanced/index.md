# Advanced JSON:API features

This topic goes beyond the basics of what's possible with JsonApiDotNetCore.

Advanced use cases are provided in the form of integration tests [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests).
This ensures they don't break during development of the framework.

Each directory typically contains:

- A set of resource types.
- A `DbContext` class to register the resource types.
- Fakers to generate deterministic test data.
- Test classes that assert the feature works as expected.
  - Entities are inserted into a randomly named PostgreSQL database.
  - An HTTP request is sent.
  - The returned response is asserted on.
  - If applicable, the changes are fetched from the database and asserted on. 

To run/debug the integration tests, follow the steps in [README.md](https://github.com/json-api-dotnet/JsonApiDotNetCore#build-from-source).
