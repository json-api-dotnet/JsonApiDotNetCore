# Example projects

Runnable example projects can be found [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/src/Examples):

- GettingStarted: A simple project with minimal configuration to develop a runnable project in minutes.
- JsonApiDotNetCoreExample: Showcases commonly-used features, such as resource definitions, atomic operations, and OpenAPI.
  - OpenApiNSwagClientExample: Uses [NSwag](https://github.com/RicoSuter/NSwag) to generate a typed OpenAPI client.
  - OpenApiKiotaClientExample: Uses [Kiota](https://learn.microsoft.com/en-us/openapi/kiota/) to generate a typed OpenAPI client.
- MultiDbContextExample: Shows how to use multiple `DbContext` classes, for connecting to multiple databases.
- DatabasePerTenantExample: Uses a different database per tenant. See [here](~/usage/advanced/multi-tenancy.md) for using multiple tenants in the same database.
- NoEntityFrameworkExample: Uses a read-only in-memory repository, instead of a real database.
- DapperExample: Uses [Dapper](https://github.com/DapperLib/Dapper) to execute SQL queries.
- ReportsExample: Uses a resource service that returns aggregated data.

> [!NOTE]
> The example projects only cover highly-requested features. More advanced use cases can be found [here](~/usage/advanced/index.md).

# Example requests

The following requests are automatically generated against the "GettingStarted" application on every deployment.

> [!NOTE]
> curl requires "[" and "]" in URLs to be escaped.

## Reading data

### Get all

[!code-ps[REQUEST](001_GET_Books.ps1)]
[!code-json[RESPONSE](001_GET_Books_Response.json)]

### Get by ID

[!code-ps[REQUEST](002_GET_Person-by-ID.ps1)]
[!code-json[RESPONSE](002_GET_Person-by-ID_Response.json)]

### Get with relationship

[!code-ps[REQUEST](003_GET_Books-including-Author.ps1)]
[!code-json[RESPONSE](003_GET_Books-including-Author_Response.json)]

### Get sparse fieldset

[!code-ps[REQUEST](004_GET_Books-PublishYear.ps1)]
[!code-json[RESPONSE](004_GET_Books-PublishYear_Response.json)]

### Filter on partial match

[!code-ps[REQUEST](005_GET_People-Filter_Partial.ps1)]
[!code-json[RESPONSE](005_GET_People-Filter_Partial_Response.json)]

### Sorting

[!code-ps[REQUEST](006_GET_Books-sorted-by-PublishYear-descending.ps1)]
[!code-json[RESPONSE](006_GET_Books-sorted-by-PublishYear-descending_Response.json)]

### Pagination

[!code-ps[REQUEST](007_GET_Books-paginated.ps1)]
[!code-json[RESPONSE](007_GET_Books-paginated_Response.json)]

## Writing data

### Create resource

[!code-ps[REQUEST](010_CREATE_Person.ps1)]
[!code-json[RESPONSE](010_CREATE_Person_Response.json)]

### Create resource with relationship

[!code-ps[REQUEST](011_CREATE_Book-with-Author.ps1)]
[!code-json[RESPONSE](011_CREATE_Book-with-Author_Response.json)]

### Update resource

[!code-ps[REQUEST](012_PATCH_Book.ps1)]
[!code-json[RESPONSE](012_PATCH_Book_Response.json)]

### Delete resource

[!code-ps[REQUEST](013_DELETE_Book.ps1)]
[!code-json[RESPONSE](013_DELETE_Book_Response.json)]
