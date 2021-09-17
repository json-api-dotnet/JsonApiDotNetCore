# OpenAPI

JsonApiDotNetCore provides an extension package that enables you to produce an [OpenAPI specification](https://swagger.io/specification/) for your JSON:API endpoints. This can be used to generate a [documentation website](https://swagger.io/tools/swagger-ui/) or to generate [client libraries](https://openapi-generator.tech/docs/generators/) in various languages. The package provides an integration with [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore).


## Getting started

1.  Install the `JsonApiDotNetCore.OpenApi` NuGet package:

    ```
    dotnet add package JsonApiDotNetCore.OpenApi
    ```

2.  Add the integration in your `Startup` class.

    ```c#
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            IMvcCoreBuilder mvcBuilder = services.AddMvcCore();

            services.AddJsonApi<AppDbContext>(mvcBuilder: mvcBuilder);

            // Adds the Swashbuckle integration.
            services.AddOpenApi(mvcBuilder);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseJsonApi();
        
            // Adds the Swashbuckle middleware.
            app.UseSwagger();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
    ```

By default, the OpenAPI specification will be available at `http://localhost:<port>/swagger/v1/swagger.json`.

## Documentation

Swashbuckle also ships with [SwaggerUI](https://swagger.io/tools/swagger-ui/), tooling for a generated documentation page. This can be enabled by installing the `Swashbuckle.AspNetCore.SwaggerUI` NuGet package and adding the following to your `Startup` class:

```c#
// Startup.cs
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseSwaggerUI();
}
```

By default, SwaggerUI will be available at `http://localhost:<port>/swagger`.

