# Error Handling

The code [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests/ExceptionHandling) shows how to customize error handling.

A user-defined exception, `ConsumerArticleIsNoLongerAvailableException`, is thrown from a resource service to demonstrate handling it.
Note that this exception can be thrown from anywhere during request execution; a resource service is just used here for simplicity.

To handle the user-defined exception, `AlternateExceptionHandler` inherits from `ExceptionHandler` to:
- Customize the JSON:API error response by adding a `meta` entry when `ConsumerArticleIsNoLongerAvailableException` is thrown.
- Indicate that `ConsumerArticleIsNoLongerAvailableException` must be logged at the Warning level.

Additionally, the `ThrowingArticle.Status` property throws an `InvalidOperationException`.
This triggers the default error handling because `AlternateExceptionHandler` delegates to its base class.
