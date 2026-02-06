> [!WARNING]
> OpenAPI support for JSON:API is currently experimental. The API and the structure of the OpenAPI document may change in future versions.

# OpenAPI documentation

After [enabling OpenAPI](~/usage/openapi.md), you can expose a documentation website with SwaggerUI, Redoc and/or Scalar.

## SwaggerUI

[SwaggerUI](https://swagger.io/tools/swagger-ui/) enables visualizing and interacting with the JSON:API endpoints through a web page.
While it conveniently provides the ability to execute requests, it doesn't show properties of derived types when component schema inheritance is used.

SwaggerUI can be enabled by installing the `Swashbuckle.AspNetCore.SwaggerUI` NuGet package and adding the following to your `Program.cs` file:

```c#
app.UseSwaggerUI();
```

Then run your app and open `/swagger` in your browser.

## Redoc

[Redoc](https://github.com/Redocly/redoc) is another popular tool that generates a documentation website from an OpenAPI document.
It lists the endpoints and their schemas, but doesn't provide the ability to execute requests.
However, this tool most accurately reflects properties when component schema inheritance is used; choosing a different "type" from the
dropdown box dynamically adapts the list of schema properties.

The `Swashbuckle.AspNetCore.ReDoc` NuGet package provides integration with Swashbuckle.
After installing the package, add the following to your `Program.cs` file:

```c#
app.UseReDoc();
```

Next, run your app and navigate to `/api-docs` to view the documentation.

## Scalar

[Scalar](https://scalar.com/) is a modern documentation website generator, which includes the ability to execute requests.
It shows component schemas in a low-level way (not collapsing `allOf` nodes), but does a poor job in handling component schema inheritance.

After installing the `Scalar.AspNetCore` NuGet package, add the following to your `Program.cs` to make it use the OpenAPI document produced by Swashbuckle:

```c#
app.MapScalarApiReference(options => options.OpenApiRoutePattern = "/swagger/{documentName}/swagger.json");
```

Then run your app and navigate to `/scalar/v1` to view the documentation.
