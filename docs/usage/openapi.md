> [!WARNING]
> OpenAPI support for JSON:API is currently experimental. The API and the structure of the OpenAPI document may change in future versions.

# OpenAPI

Exposing an [OpenAPI document](https://swagger.io/specification/) for your JSON:API endpoints enables to provide a
[documentation website](https://swagger.io/tools/swagger-ui/) and to generate typed
[client libraries](https://openapi-generator.tech/docs/generators/) in various languages.

The [JsonApiDotNetCore.OpenApi.Swashbuckle](https://github.com/json-api-dotnet/JsonApiDotNetCore/pkgs/nuget/JsonApiDotNetCore.OpenApi.Swashbuckle) NuGet package
provides OpenAPI support for JSON:API by integrating with [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore).

## Getting started

1.  Install the `JsonApiDotNetCore.OpenApi.Swashbuckle` NuGet package:

    ```
    dotnet add package JsonApiDotNetCore.OpenApi.Swashbuckle --prerelease
    ```

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

> [!TIP]
> In addition to the documentation here, various examples can be found in the [OpenApiTests project](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/OpenApiTests).

### Customizing the Route Template

Because Swashbuckle doesn't properly implement the ASP.NET Options pattern, you must *not* use its
[documented way](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/docs/configure-and-customize-swagger.md#change-the-path-for-swagger-json-endpoints)
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

You can combine this with the documentation that Swashbuckle itself supports, by enabling it as described
[here](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/docs/configure-and-customize-swaggergen.md#include-descriptions-from-xml-comments).
This adds documentation for additional types, such as triple-slash comments on enums used in your resource models.

## Custom JSON:API action methods

To express the metadata of [custom action methods](~/usage/extensibility/controllers.md#custom-action-methods) in OpenAPI,
use the following attributes on your controller action method:

- The `Name` property on `HttpMethodAttribute` to specify the OpenAPI operation ID, for example:
  ```c#
  [HttpGet("active", Name = "get-active-users")]
  ```

- `EndpointDescriptionAttribute` to specify the OpenAPI endpoint description, for example:
  ```c#
  [EndpointDescription("Provides access to user accounts.")]
  ```

- `ConsumesAttribute` to specify the resource type of the request body, for example:
  ```c#
  [Consumes(typeof(UserAccount), "application/vnd.api+json")]
  ```
  > [!NOTE]
  > The `contentType` parameter is required, but effectively ignored.

- `ProducesResponseTypeAttribute` attribute(s) to specify the response types and status codes, for example:
  ```c#
  [ProducesResponseType<ICollection<UserAccount>>(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status409Conflict)]
  ```
  > [!NOTE]
  > For non-success response status codes, the type should be omitted.

Custom parameters on action methods can be decorated with the usual attributes, such as `[Required]`, `[Description]`, etc.
