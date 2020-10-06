# Layer Overview

By default, data retrieval is distributed across three layers:

```
JsonApiController (required)

+-- JsonApiResourceService : IResourceService

     +-- EntityFrameworkCoreRepository : IResourceRepository
```

Customization can be done at any of these layers. However, it is recommended that you make your customizations at the service or the repository layer when possible, to keep the controllers free of unnecessary logic.
You can use the following as a general rule of thumb for where to put business logic:

- `Controller`: simple validation logic that should result in the return of specific HTTP status codes, such as model validation
- `IResourceService`: advanced business logic and replacement of data access mechanisms
- `IResourceRepository`: custom logic that builds on the Entity Framework Core APIs

## Replacing Services

**Note:** If you are using auto-discovery, resource services and repositories will be automatically registered for you.

Replacing services and repositories is done on a per-resource basis and can be done through dependency injection in your Startup.cs file.

```c#
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddScoped<ProductService, IResourceService<Product>();
    services.AddScoped<ProductRepository, IResourceRepository<Product>>();
}
```
