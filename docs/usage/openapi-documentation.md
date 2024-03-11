# OpenAPI documentation

After [enabling OpenAPI](~/usage/openapi.md), you can expose a documentation website with SwaggerUI or Redoc.

### SwaggerUI

Swashbuckle ships with [SwaggerUI](https://swagger.io/tools/swagger-ui/), which enables to visualize and interact with the JSON:API endpoints through a web page.
This can be enabled by installing the `Swashbuckle.AspNetCore.SwaggerUI` NuGet package and adding the following to your `Program.cs` file:

```c#
app.UseSwaggerUI();
```

By default, SwaggerUI will be available at `http://localhost:<port>/swagger`.

### Redoc

[Redoc](https://github.com/Redocly/redoc) is another popular tool that generates a documentation website from an OpenAPI document.
It lists the endpoints and their schemas, but doesn't provide the ability to execute requests.
The `Swashbuckle.AspNetCore.ReDoc` NuGet package provides integration with Swashbuckle.
