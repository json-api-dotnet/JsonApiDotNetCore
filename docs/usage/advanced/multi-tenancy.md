# Multi-tenancy

The code [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests/MultiTenancy) shows how to handle multiple tenants in a single database.

> [!TIP]
> To use a different database per tenant, see [this](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/src/Examples/DatabasePerTenantExample) example instead.
> Its `DbContext` dynamically sets the connection string per request. This requires the database structure to be identical for all tenants.

The essence of implementing multi-tenancy within a single database is instructing Entity Framework Core to add implicit filters when entities are queried.
See the usage of `HasQueryFilter` in the `DbContext` class. It injects an `ITenantProvider` to determine the active tenant for the current HTTP request.

> [!NOTE]
> For simplicity, this example uses a route parameter to indicate the active tenant.
> Provide your own `ITenantProvider` to determine the tenant from somewhere else, such as the incoming OAuth token.

The generic `MultiTenantResourceService` transparently sets the tenant ID when creating a new resource.
Furthermore, it performs extra queries to ensure relationship changes apply to the current tenant, and to produce better error messages.

While `MultiTenantResourceService` is used for both resource types, _only_ the `WebShop` resource type implements `IHasTenant`.
The related resource type `WebProduct` does not. Because the products table has a foreign key to the (tenant-specific) shop it belongs to, it doesn't need a `TenantId` column.
When a JSON:API request for web products executes, the `HasQueryFilter` in the `DbContext` ensures that only products belonging to the tenant-specific shop are returned.
