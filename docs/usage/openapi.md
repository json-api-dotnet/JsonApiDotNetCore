# OpenAPI

Exposing an [OpenAPI document](https://swagger.io/specification/) for your JSON:API endpoints enables to provide a
[documentation website](https://swagger.io/tools/swagger-ui/) and to generate typed
[client libraries](https://openapi-generator.tech/docs/generators/) in various languages.

The [JsonApiDotNetCore.OpenApi.Swashbuckle](https://github.com/json-api-dotnet/JsonApiDotNetCore/pkgs/nuget/JsonApiDotNetCore.OpenApi.Swashbuckle) NuGet package
provides OpenAPI support for JSON:API by integrating with [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore).

## Getting started

1.  Install the `JsonApiDotNetCore.OpenApi.Swashbuckle` NuGet package:

    ```
    dotnet add package JsonApiDotNetCore.OpenApi.Swashbuckle
    ```

    > [!NOTE]
    > Because this package is still experimental, it's not yet available on NuGet.
    > Use the steps [here](https://github.com/json-api-dotnet/JsonApiDotNetCore?tab=readme-ov-file#trying-out-the-latest-build) to install.

2.  Add the JSON:API support to your `Program.cs` file.

    ```c#
    builder.Services.AddJsonApi<AppDbContext>();

    // Configure Swashbuckle for JSON:API.
    builder.Services.AddOpenApiForJsonApi();

    var app = builder.Build();

    app.UseRouting();
    app.UseJsonApi();

    // Add the Swashbuckle middleware.
    app.UseSwagger();
    ```

By default, the OpenAPI document will be available at `http://localhost:<port>/swagger/v1/swagger.json`.

### Customizing the Route Template

Because Swashbuckle doesn't properly implement the ASP.NET Options pattern, you must *not* use its
[documented way](https://github.com/domaindrivendev/Swashbuckle.AspNetCore?tab=readme-ov-file#change-the-path-for-swagger-json-endpoints)
to change the route template:

```c#
// DO NOT USE THIS! INCOMPATIBLE WITH JSON:API!
app.UseSwagger(options => options.RouteTemplate = "api-docs/{documentName}/swagger.yaml");
```

Instead, always call `UseSwagger()` *without parameters*. To change the route template, use the code below:

```c#
builder.Services.Configure<SwaggerOptions>(options => options.RouteTemplate = "/api-docs/{documentName}/swagger.yaml");
```

If you want to inject dependencies to set the route template, use:

```c#
builder.Services.AddOptions<SwaggerOptions>().Configure<IServiceProvider>((options, serviceProvider) =>
{
    var webHostEnvironment = serviceProvider.GetRequiredService<IWebHostEnvironment>();
    string appName = webHostEnvironment.ApplicationName;
    options.RouteTemplate = $"/api-docs/{{documentName}}/{appName}-swagger.yaml";
});
```

## Triple-slash comments

Documentation for JSON:API endpoints is provided out of the box, which shows in SwaggerUI and through IDE IntelliSense in auto-generated clients.
To also get documentation for your resource classes and their properties, add the following to your project file.
The `NoWarn` line is optional, which suppresses build warnings for undocumented types and members.

```xml
  <PropertyGroup>
    <GenerateDocumentationFile>True</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
  </PropertyGroup>
```

You can combine this with the documentation that Swagger itself supports, by enabling it as described
[here](https://github.com/domaindrivendev/Swashbuckle.AspNetCore#include-descriptions-from-xml-comments).
This adds documentation for additional types, such as triple-slash comments on enums used in your resource models.
