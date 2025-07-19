# Soft Deletion

The code [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests/SoftDeletion) demonstrates how to implement soft deletion of resources.

> [!TIP]
> This scenario is comparable with [Archiving](~/usage/advanced/archiving.md).
> The difference is that soft-deleted resources are never accessible by JSON:API clients (despite still being stored in the database), whereas archived resources _are_ accessible.

The essence of implementing soft deletion is instructing Entity Framework Core to add implicit filters when entities are queried.
See the usage of `HasQueryFilter` in the `DbContext` class.

The `ISoftDeletable` interface provides the `SoftDeletedAt` database column. The `Company` and `Department` resource types implement this interface to indicate they use soft deletion.

The generic `SoftDeletionAwareResourceService` overrides the `DeleteAsync` method to soft-delete a resource instead of truly deleting it, if it implements `ISoftDeletable`.
Furthermore, it performs extra queries to ensure relationship changes do not reference soft-deleted resources, and to produce better error messages.
