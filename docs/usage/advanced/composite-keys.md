# Composite Keys

The code [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests/CompositeKeys) shows how database tables with composite keys can be used.

- The `DbContext` configures `Car` to have a composite primary key consisting of the `RegionId` and `LicensePlate` columns.
- The `Car.Id` property is overridden to provide a unique ID for JSON:API. It is marked with `[NotMapped]`, meaning no `Id` column exists in the database table.
- The `Engine` and `Dealership` resource types define relationships that generate composite foreign keys in the database.
- A custom resource repository is used to rewrite IDs from filter/sort query string parameters into `RegionId` and `LicensePlate` lookups.
