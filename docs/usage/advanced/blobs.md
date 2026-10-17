# BLOBs

The code [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests/Blobs) shows how Binary Large Objects (BLOBs) can be used.

- The `ImageContainer` resource type contains nullable and non-nullable `byte[]` properties.
- BLOBs are queried and persisted using Entity Framework Core.
- The BLOB data is returned as a base-64 encoded string in the JSON response.

Blobs are handled automatically; there's no need for custom code.

## Binary data as a collection

_since v6.0_

A `byte[]` attribute is a [collection attribute](~/usage/resources/attributes.md#collection-attributes) of bytes that is written as a base-64 string. This has the following consequences:

- Change detection compares the content of the bytes. Sending an identical blob in a PATCH request is not considered a change, so `204 No Content` is returned when nothing else was changed.
- The collection functions from [filtering](~/usage/reading/filtering.md) are available. For example, `filter=has(data)` returns resources that have a non-empty blob,
  `filter=greaterThan(count(data),'3')` selects blobs that are larger than 3 bytes, and `sort=-count(data)` sorts by size.
  `filter=has(data,equals(_,'137'))` matches blobs that contain a specific byte value.
  A `null` blob counts as empty.
- Whether such filters can be translated to SQL depends on the JSON and array support of your database provider.

Other collections of bytes, such as `List<byte>`, are collection attributes as well.
