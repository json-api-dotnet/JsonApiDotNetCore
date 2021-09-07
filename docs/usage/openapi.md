# OpenAPI

You can describe your API with an OpenAPI specification using the [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) integration for JsonApiDotNetCore. 

## Installation

Install the `JsonApiDotNetCore.OpenApi` NuGet package.

### CLI

```
dotnet add package JsonApiDotNetCore.OpenApi
```

### Visual Studio

```powershell
Install-Package JsonApiDotNetCore.OpenApi
```

### *.csproj

```xml
<ItemGroup>
  <!-- Be sure to check NuGet for the latest version # -->
  <PackageReference Include="JsonApiDotNetCore.OpenApi" Version="4.0.0" />
</ItemGroup>
```

## Usage

Add the integration in your `Startup` class.

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

Swashbuckle also ships with [SwaggerUI](https://swagger.io/tools/swagger-ui/), tooling for a generated documentation page. This can be enabled by installing the `Swashbuckle.AspNetCore.SwaggerUI` NuGet package and adding the following to your `Startup` class.

```c#
// Startup.cs
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseSwaggerUI();
}
```

By default, SwaggerUI will be available at `http://localhost:<port>/swagger`.

