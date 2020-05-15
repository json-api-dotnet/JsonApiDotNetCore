# Layer Overview

By default, data retrieval is distributed across 3 layers:

```
JsonApiController (required)

+-- DefaultResourceService: IResourceService

     +-- DefaultResourceRepository: IResourceRepository
```

Customization can be done at any of these layers. However, it is recommended that you make your customizations at the service or the repository layer when possible to keep the controllers free of unnecessary logic.
You can use the following as a general rule of thumb for where to put business logic:

- `Controller`: simple validation logic that should result in the return of specific HTTP status codes, such as model validation
- `IResourceService`: advanced business logic and replacement of data access mechanisms
- `IResourceRepository`: custom logic that builds on the Entity Framework Core APIs, such as Authorization of data

## Replacing Services

**Note:** If you are using auto-discovery, services will be automatically registered for you.

Replacing services is done on a per-resource basis and can be done through simple DI in your Startup.cs file.

In v3.0.0 we introduced an extenion method that you should use to
register services. This method handles some of the common issues
users have had with service registration.

```c#
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // custom service
    services.AddResourceService<FooService>();

    // custom repository
    services.AddScoped<AFooRepository>();
}
```

Prior to v3.0.0 you could do it like so:

```c#
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // custom service
    services.AddScoped<IResourceRepository<Foo>, FooService>();

    // custom repository
    services.AddScoped<IResourceRepository<Foo>, FooService>();

    // ...
}
```
