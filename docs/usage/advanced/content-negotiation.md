# Content Negotiation

The code [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests/ContentNegotiation) demonstrates how content negotiation in JSON:API works.

Additionally, the code [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests/ContentNegotiation/CustomExtensions) provides
a custom "server-time" JSON:API extension that returns the local or UTC server time in top-level `meta`.
- This extension can be used in the `Accept` and `Content-Type` HTTP headers.
- In a request body, the optional `useLocalTime` property in top-level `meta` indicates whether to return the local or UTC time.

This feature is implemented using the following extensibility points:

- At startup, the "server-time" extension is added in `JsonApiOptions`, which permits clients to use it.
- A custom `JsonApiContentNegotiator` chooses which extensions are active for an incoming request, taking the "server-time" extension into account.
- A custom `IDocumentAdapter` captures the incoming request body, providing access to the `useLocalTime` property in `meta`.
- A custom `IResponseMeta` adds the server time to the response, depending on the activated extensions in `IJsonApiRequest` and the captured request body.
