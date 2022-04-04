# JsonApiDotNetCore roadmap

This document provides an overview of the direction this project is heading and lists what we intend to work on in the near future.

> Disclaimer: This is an open source project. The available time of our contributors varies and therefore we do not plan release dates. This document expresses our current intent, which may change over time.

## v4.x

We've completed active development on v4.x, but we'll still fix important bugs or add small enhancements on request that don't require breaking changes nor lots of testing.

## v5.x

The need for breaking changes has blocked several efforts in the v4.x release, so now that we're starting work on v5, we're going to catch up.

- [x] Remove Resource Hooks [#1025](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/1025)
- [x] Update to .NET 5 with EF Core 5 [#1026](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/1026)
- [x] Native many-to-many [#935](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/935)
- [x] Refactorings [#1027](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/1027) [#944](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/944)
- [x] Tweak trace logging [#1033](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/1033)
- [x] Instrumentation [#1032](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/1032)
- [x] Optimized delete to-many [#1030](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/1030)
- [x] Support System.Text.Json [#664](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/664) [#999](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/999) [1077](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/1077) [1078](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/1078)
- [x] Optimize IIdentifiable to ResourceObject conversion [#1028](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/1028) [#1024](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/1024) [#233](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/233)
- [x] Nullable reference types [#1029](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/1029)
- [x] Improved paging links [#1010](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/1010)
- [x] Configuration validation [#170](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/170)
- [x] Auto-generated controllers [#732](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/732) [#365](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/365)
- [x] Support .NET 6 with EF Core 6 [#1109](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/1109)
- [x] Extract annotations into separate package [#730](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/730)
- [x] Resource inheritance [#844](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/844)

Aside from the list above, we have interest in the following topics. It's too soon yet to decide whether they'll make it into v5.x or in a later major version.

- Optimistic concurrency [#1004](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/1004)
- OpenAPI (Swagger) [#1046](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/1046)
- Fluent API [#776](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/776)
- Idempotency

## Feedback

The best way to give feedback is to create new issues or upvote/downvote existing ones.
Please give us feedback that will give us insight on the following points:

* Existing features that are missing some capability or otherwise don't work well enough.
* Missing features that should be added to the product.
* Design choices for a feature that is currently in-progress.
