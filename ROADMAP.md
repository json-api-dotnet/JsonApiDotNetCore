# JsonApiDotNetCore roadmap

This document provides an overview of the direction this project is heading and lists what we intend to work on in the near future.

> Disclaimer: This is an open source project. The available time of our contributors varies and therefore we do not plan release dates. This document expresses our current intent, which may change over time.

## v4.x
In December 2020, we released v4.0 stable after a long time. From now on, we'd like to release new features and bugfixes often.
In subsequent v4.x releases, we intend to implement the next features in non-breaking ways.

- Codebase improvements (refactor tests [#715](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/715), coding guidelines [#835](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/835) [#290](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/290), cibuild [#908](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/908))
- Bulk/batch support (atomic:operations) [#936](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/936)
- Write callbacks [#934](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/934)
- ETags [#933](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/933)
- Optimistic Concurrency [#350](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/350)
- Configuration validation [#414](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/414) [#170](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/170)

## vNext
We have interest in the following topics for future versions.
Some cannot be done in v4.x the way we'd like without introducing breaking changes.
Others require more exploration first, or depend on other features.

- Resource inheritance [#844](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/844)
- Split into multiple NuGet packages [#730](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/730) [#661](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/661) [#292](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/292)
- System.Text.Json [#664](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/664)
- EF Core 5 Many-to-many relationships [#935](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/935)
- Fluent API [#776](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/776)
- Auto-generated controllers [#732](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/732) [#365](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/365)
- Serialization, discovery and documentation [#661](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/661) [#259](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/259)

## Feedback

The best way to give feedback is to create new issues or upvote/downvote existing ones.
Please give us feedback that will give us insight on the following points:

* Existing features that are missing some capability or otherwise don't work well enough.
* Missing features that should be added to the product.
* Design choices for a feature that is currently in-progress.
