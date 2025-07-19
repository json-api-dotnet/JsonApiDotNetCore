# ID Obfuscation

The code [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests/IdObfuscation) shows how to use obfuscated IDs.
They are typically used to prevent clients from guessing primary key values.

All IDs sent by clients are transparently de-obfuscated into internal numeric values before accessing the database.
Numeric IDs returned from the database are obfuscated before they are sent to the client.

> [!NOTE]
> An alternate solution is to use GUIDs instead of numeric primary keys in the database.

ID obfuscation is achieved using the following extensibility points:

- For simplicity, `HexadecimalCodec` is used to obfuscate numeric IDs to a hexadecimal format. A more realistic use case would be to use a symmetric crypto algorithm.
- `ObfuscatedIdentifiable` acts as the base class for resource types, handling the obfuscation and de-obfuscation of IDs.
- `ObfuscatedIdentifiableController` acts as the base class for controllers. It inherits from `BaseJsonApiController`, changing the `id` parameter in action methods to type `string`.
