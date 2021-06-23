# JsonApiDotNetCore versioning policy

Basically, we strive to adhere to [semantic versioning](https://semver.org/).

However, we believe that our userbase is still small enough to allow for some flexibility in _minor_ updates, see below.

## Major updates

For breaking changes in _major_ updates, we intend to list the ones that may affect user code or the clients of an API in our release notes. We also include important changes between versions in our published documentation.

## Minor updates

We **WILL NOT** introduce breaking changes in _minor_ updates on our common extensibility points such as controllers, resource services, resource repositories, resource definitions, and `Identifiable` as well as common annotations such as `[Attr]`, `[HasOne]`, `[HasMany]`, and `[HasManyThrough]`. The same applies to the URL routes, JSON structure of request/response bodies, and query string syntax.

In previous versions of JsonApiDotNetCore, almost everything was public. While that makes customizations very easy for users, it kinda puts us in a corner: nearly every change would require a new major version. Therefore we try to balance between adding new features to the next _minor_ version or postpone them to the next _major_ version. This means we **CAREFULLY CONSIDER** if we can prevent breaking changes in _minor_ updates to signatures of "pubternal" types (public types in `Internal` namespaces), as well as exposed types of which we believe users are unlikely to have taken a direct dependency on. One example would be to inject an additional dependency in the constructor of a not-so-well-known class, such as `IncludedResourceObjectBuilder`. In the unlikely case that a user has taken a dependency on this class, the compiler error message is clear and the fix is obvious and easy. We may introduce binary breaking changes (such as adding an optional constructor parameter to a custom exception type), which requires users to recompile their existing code.

Our goal is to try to minimize such breaks and require only a recompile of existing API projects. This also means that we'll need to publish an updated release for [JsonApiDotNetCore.MongoDb](https://github.com/json-api-dotnet/JsonApiDotNetCore.MongoDb) when this happens.

We may also correct error messages in _minor_ updates.

## Backports

When users report that they are unable to upgrade as a result of breaking changes in a _minor_ version, we're willing to consider backporting fixes they need to an earlier _minor_ version.
