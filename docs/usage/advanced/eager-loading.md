# Eager Loading Related Resources

The code [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests/EagerLoading) uses the `[EagerLoad]` attribute to facilitate calculated properties that depend on related resources.
The related resources are fetched from the database, but not returned to the client unless explicitly requested using the `include` query string parameter.

- The `Street` resource type uses `EagerLoad` on its `Buildings` to-many relationship because its `DoorTotalCount` calculated property depends on it.
- The `Building` resource type uses `EagerLoad` on its `Windows` to-many relationship because its `WindowCount` calculated property depends on it.
- The `Building` resource type uses `EagerLoad` on its `PrimaryDoor` to-one required relationship because its `PrimaryDoorColor` calculated property depends on it.
  - Because this is a required relationship, special handling occurs in `Building`, `BuildingRepository`, and `BuildingDefinition`.
- The `Building` resource type uses `EagerLoad` on its `SecondaryDoor` to-one optional relationship because its `SecondaryDoorColor` calculated property depends on it.

As can be seen from the usages above, a chain of `EagerLoad` attributes can result in fetching a chain of related resources from the database.
