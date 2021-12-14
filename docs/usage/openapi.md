# OpenAPI

JsonApiDotNetCore provides an extension package that enables you to produce an [OpenAPI specification](https://swagger.io/specification/) for your JSON:API endpoints. This can be used to generate a [documentation website](https://swagger.io/tools/swagger-ui/) or to generate [client libraries](https://openapi-generator.tech/docs/generators/) in various languages. The package provides an integration with [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore).


## Getting started

1.  Install the `JsonApiDotNetCore.OpenApi` NuGet package:

    ```
    dotnet add package JsonApiDotNetCore.OpenApi
    ```

2.  Add the integration in your `Program.cs` file.

    ```c#
    IMvcCoreBuilder mvcCoreBuilder = builder.Services.AddMvcCore();

    builder.Services.AddJsonApi<AppDbContext>(mvcBuilder: mvcCoreBuilder);

    // Configures Swashbuckle for JSON:API.
    builder.Services.AddOpenApi(mvcCoreBuilder);

    var app = builder.Build();

    app.UseRouting();
    app.UseJsonApi();

    // Adds the Swashbuckle middleware.
    app.UseSwagger();
    ```

By default, the OpenAPI specification will be available at `http://localhost:<port>/swagger/v1/swagger.json`.

## Documentation

Swashbuckle also ships with [SwaggerUI](https://swagger.io/tools/swagger-ui/), tooling for a generated documentation page. This can be enabled by installing the `Swashbuckle.AspNetCore.SwaggerUI` NuGet package and adding the following to your `Program.cs` file:

```c#
app.UseSwaggerUI();
```

By default, SwaggerUI will be available at `http://localhost:<port>/swagger`.
