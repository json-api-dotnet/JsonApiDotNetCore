# ASP.NET Model Validation

The code [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests/InputValidation/ModelState) shows how to use [ASP.NET Model Validation](https://learn.microsoft.com/aspnet/web-api/overview/formats-and-model-binding/model-validation-in-aspnet-web-api) attributes.

> [!TIP]
> See [Atomic Operations](~/usage/advanced/operations.md) for how to implement a custom model validator.

The resource types are decorated with Model Validation attributes, such as `[Required]`, `[RegularExpression]`, `[MinLength]`, and `[Range]`.

Only the fields that appear in a request body (partial POST/PATCH) are validated.
When validation fails, the source pointer in the response indicates which attribute(s) are invalid.

Model Validation is enabled by default, but can be [turned off in options](~/usage/options.md#modelstate-validation).
Aside from adding validation attributes to your resource properties, no further code is needed.
