# Archiving

The code [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests/Archiving) demonstrates how to implement archived resources.

> [!TIP]
> This scenario is comparable with [Soft Deletion](~/usage/advanced/soft-deletion.md).
> The difference is that archived resources are accessible to JSON:API clients, whereas soft-deleted resources _never_ are.

- Archived resources can be fetched by ID, but don't show up in searches by default.
- Resources can only be created in a non-archived state and then archived/unarchived using a PATCH resource request.
- The archive date is stored in the database, but cannot be modified through JSON:API.
- To delete a resource, it must be archived first.

This feature is implemented using a custom resource definition. It intercepts write operations and recursively scans incoming filters.
