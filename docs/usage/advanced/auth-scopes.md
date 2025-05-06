# Authorization Scopes

The code [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests/Authorization/Scopes) shows how scope-based authorization can be used.

- For simplicity, this code assumes the granted scopes are passed in a plain-text HTTP header. A more realistic use case would be to obtain the scopes from an OAuth token.
- The HTTP header lists which resource types can be read from and/or written to.
- An [ASP.NET Action Filter](https://learn.microsoft.com/aspnet/core/mvc/controllers/filters) validates incoming JSON:API resource/relationship requests.
  - The incoming request path is validated against the permitted read/write permissions per resource type.
  - The resource types used in query string parameters are validated against the permitted set of resource types.
- A customized operations controller verifies that all incoming operations are allowed.
