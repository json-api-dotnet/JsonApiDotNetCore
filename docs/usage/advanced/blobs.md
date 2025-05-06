# BLOBs

The code [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests/Blobs) shows how Binary Large Objects (BLOBs) can be used.

- The `ImageContainer` resource type contains nullable and non-nullable `byte[]` properties.
- BLOBs are queried and persisted using Entity Framework Core.
- The BLOB data is returned as a base-64 encoded string in the JSON response.

Blobs are handled automatically; there's no need for custom code.
