# Injecting services in resource types

The code [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests/ResourceConstructorInjection) shows how to inject services into resource types.

Because Entity Framework Core doesn't support injecting arbitrary services into entity types (only a few special types), a workaround is used.
Instead of injecting the desired services directly, the `DbContext` is injected, which injects the desired services and exposes them via properties.

- The `PostOffice` and `GiftCertificate` resource types both inject the `DbContext` in their constructors.
- The `DbContext` injects `TimeProvider` and exposes it through a property.
- `GiftCertificate` obtains the `TimeProvider` via the `DbContext` property to calculate the value for its exposed `HasExpired` property, which depends on the current time.
- `PostOffice` obtains the `TimeProvider` via the `DbContext` property to calculate the value for its exposed `IsOpen` property, which depends on the current time.
