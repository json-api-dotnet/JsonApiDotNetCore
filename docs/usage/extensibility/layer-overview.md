# Layer Overview

By default, data access flows through the next three layers:

```
JsonApiController (required)
  JsonApiResourceService : IResourceService
     EntityFrameworkCoreRepository : IResourceRepository
```

Aside from these pluggable endpoint-oriented layers, we provide a resource-oriented extensibility point:

```
JsonApiResourceDefinition : IResourceDefinition
```

Resource definition callbacks are invoked from the built-in resource service/repository layers, as well as from the serializer.
For example, `IResourceDefinition.OnSerialize` is invoked whenever a resource is sent back to the client, irrespective of the endpoint.
Likewise, `IResourceDefinition.OnSetToOneRelationshipAsync` is called from a patch-resource-with-relationships endpoint, as well as from patch-relationship.

Customization can be done at any of these extensibility points. It is usually sufficient to place your business logic in a resource definition, but depending
on your needs, you may want to replace other parts by deriving from the built-in classes and override virtual methods or call their protected base methods.

## Replacing injected services

**Note:** If you are using auto-discovery, then resource services, repositories and resource definitions will be automatically registered for you.

Replacing built-in services is done on a per-resource basis and can be done through dependency injection in your Startup.cs file.
For convenience, extension methods are provided to register layers on all their implemented interfaces.

```c#
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddResourceService<ProductService>();
    services.AddResourceRepository<ProductRepository>();
    services.AddResourceDefinition<ProductDefinition>();

    services.AddScoped<IResourceFactory, CustomResourceFactory>();
    services.AddScoped<IJsonApiSerializerFactory, CustomResponseSerializerFactory>();
}
```
