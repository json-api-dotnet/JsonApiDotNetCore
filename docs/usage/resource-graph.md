# The Resource Graph

_NOTE: prior to 3.0.0 this was called the `ContextGraph`_

The `ResourceGraph` is a map of all the json:api resources and their relationships that your API serves.

It is built at app startup and available as a singleton through Dependency Injection.

## Constructing The Graph

There are three ways the resource graph can be created:

1. Manually specifying each resource
2. Specifying an entire DbContext
3. Auto-discovery

It is also possible to combine the three of them at once. Be aware that some configuration might overlap, 
for example you could manually add a resource to the graph which is also auto-discovered. In such a scenario, the configuration
is prioritized by the order of the list above.

### Manual Specification

You can manually construct the graph.

```c#
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddJsonApi(resources: builder =>
    {
        builder.AddResource<Person>();
    });
}
```

### Specifying an Entity Framework Core DbContext

If you are using Entity Framework Core as your ORM, you can add an entire `DbContext` with one line.

```c#
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddJsonApi<AppDbContext>();
}
```

Be aware that the previous command does not inject resource definitions and service layer overrides. You can combine it with auto-discovery to register them.

```c#
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddJsonApi<AppDbContext>(
        options => { /* ... */ },
        discovery => discovery.AddCurrentAssembly());
}
```

### Auto-discovery

Auto-discovery refers to the process of reflecting on an assembly and
detecting all of the json:api resources and services.

The following command will build the resource graph using all `IIdentifiable`
implementations. It also injects resource definitions and service layer overrides which we will
cover in a later section. You can enable auto-discovery for the
current assembly by adding the following to your `Startup` class.

```c#
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddJsonApi(
        options => { /* ... */ },
        discovery => discovery.AddCurrentAssembly());
}
```

### Public Resource Name

The public resource name is exposed through the `type` member in the json:api payload. This can be configured by the following approaches (in order of priority):

1. The `publicResourceName` option when manually adding a resource to the graph
```c#
services.AddJsonApi(resources: builder =>
{
    builder.AddResource<Person>(publicResourceName: "people");
});
```

2. The model is decorated with a `ResourceAttribute`
```c#
[Resource("myResources")]
public class MyModel : Identifiable { /* ... */ }
```

3. The configured naming convention (by default this is camel-case).
```c#
// this will be registered as "myModels"
public class MyModel : Identifiable { /* ... */ }
```
This convention can be changed by setting the `SerializerSettings` property on `IJsonApiOptions`.
```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddJsonApi(
        options =>
        {
            options.SerializerSettings.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new KebabCaseNamingStrategy()
            }
        });
}
```
