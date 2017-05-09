---
currentMenu: layers
---

# The Layers

By default, data retrieval is distributed across 3 layers:

1. [JsonApiController](https://github.com/Research-Institute/json-api-dotnet-core/blob/master/src/JsonApiDotNetCore/Controllers/JsonApiController.cs) (required)
2. [IResourceService](https://github.com/Research-Institute/json-api-dotnet-core/blob/master/src/JsonApiDotNetCore/Services/IResourceService.cs) (default [EntityResourceService](https://github.com/Research-Institute/json-api-dotnet-core/blob/master/src/JsonApiDotNetCore/Services/EntityResourceService.cs))
3. [IEntityRepository](https://github.com/Research-Institute/json-api-dotnet-core/blob/master/src/JsonApiDotNetCore/Data/IEntityRepository.cs) (default [DefaultEntityRepository](https://github.com/Research-Institute/json-api-dotnet-core/blob/master/src/JsonApiDotNetCore/Data/DefaultEntityRepository.cs))

Customization can be done at any of these layers. However, it is recommended that you make your customizations at the service or the repository layer when possible to keep the controllers free of unnecessary logic. You can use the following as a general rule of
thumb for where to put business logic:

- **Controller**: simple validation logic that should result in the return of specific HTTP status codes such as model validation
- **IResourceService**: advanced BL and replacement of data access mechanisms
- **IEntityRepository**: custom logic that builds on the EF APIs, such as Authorization of data

## Replacing Services / Repositories

Replacing services is done on a per resource basis and can be done through simple DI
in your Startup.cs file:

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services)
{
    // custom service
    services.AddScoped<IEntityRepository<Person>, CustomPersonService>();

    // custom repository
    services.AddScoped<IEntityRepository<TodoItem>, AuthorizedTodoItemRepository>();

    // ...
}
```

## Not Using Entity Framework?

Out of the box, the library uses your `DbContext` to create a "ContextGraph" or map of all your models and their relationships. If, however, you have models that are not members of a `DbContext`, you can manually create this graph like so:

```csharp
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // Add framework services.
    var mvcBuilder = services.AddMvc();

    services.AddJsonApi(options => {
        options.Namespace = "api/v1";
        options.BuildContextGraph((builder) => {
            builder.AddResource<MyModel>("my-models");
        });
    }, mvcBuilder);
    // ...
}
```
