# OpenAPI

JsonApiDotNetCore provides an extension package that enables you to produce an [OpenAPI specification](https://swagger.io/specification/) for your JSON:API endpoints.
This can be used to generate a [documentation website](https://swagger.io/tools/swagger-ui/) or to generate [client libraries](https://openapi-generator.tech/docs/generators/) in various languages.
The package provides an integration with [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore).


## Getting started

1.  Install the `JsonApiDotNetCore.OpenApi` NuGet package:

    ```
    dotnet add package JsonApiDotNetCore.OpenApi
    ```

2.  Add the integration in your `Program.cs` file.

    ```c#
    builder.Services.AddJsonApi<AppDbContext>();

    // Configure Swashbuckle for JSON:API.
    builder.Services.AddOpenApi();

    var app = builder.Build();

    app.UseRouting();
    app.UseJsonApi();

    // Add the Swashbuckle middleware.
    app.UseSwagger();
    ```

By default, the OpenAPI specification will be available at `http://localhost:<port>/swagger/v1/swagger.json`.

## Documentation

### SwaggerUI

Swashbuckle also ships with [SwaggerUI](https://swagger.io/tools/swagger-ui/), which enables to visualize and interact with the API endpoints through a web page.
This can be enabled by installing the `Swashbuckle.AspNetCore.SwaggerUI` NuGet package and adding the following to your `Program.cs` file:

```c#
app.UseSwaggerUI();
```

By default, SwaggerUI will be available at `http://localhost:<port>/swagger`.

### Triple-slash comments

Documentation for JSON:API endpoints is provided out of the box, which shows in SwaggerUI and through IDE IntelliSense in auto-generated clients.
To also get documentation for your resource classes and their properties, add the following to your project file.
The `NoWarn` line is optional, which suppresses build warnings for undocumented types and members.

```xml
  <PropertyGroup>
    <GenerateDocumentationFile>True</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
  </PropertyGroup>
```

You can combine this with the documentation that Swagger itself supports, by enabling it as described [here](https://github.com/domaindrivendev/Swashbuckle.AspNetCore#include-descriptions-from-xml-comments).
This adds documentation for additional types, such as triple-slash comments on enums used in your resource models.
