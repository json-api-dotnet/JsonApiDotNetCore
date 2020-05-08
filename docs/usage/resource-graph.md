# The Resource Graph

_NOTE: prior to 3.0.0 this was called the `ContextGraph`_

The `ResourceGraph` is a map of all the json:api resources and their relationships that your API serves.

It is built at app startup and available as a singleton through Dependency Injection.

## Constructing The Graph

There are three ways the resource graph can be created:

1. Auto-discovery
2. Specifying an entire DbContext
3. Manually specifying each resource

### Auto-Discovery

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

### Entity Framework Core DbContext

If you are using Entity Framework Core as your ORM, you can add an entire `DbContext` with one line.

```c#
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddJsonApi<AppDbContext>();
}
```

Be aware that the previous command does not inject resource definitions and service layer overrides. You can it combine with auto-discovery to register them.

```c#
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddJsonApi<AppDbContext>(
        options => { /* ... */ },
        discovery => discovery.AddCurrentAssembly());
}
```

### Manual Specification

You can also manually construct the graph.

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

### Public Resource Type Name

The public resource type name is determined by the following criteria (in order of priority):

1. The model is decorated with a `ResourceAttribute`
```c#
[Resource("my-models")]
public class MyModel : Identifiable { /* ... */ }
```

2. The configured naming convention (by default this is camel-case).
```c#
// this will be registered as "myModels"
public class MyModel : Identifiable { /* ... */ }
```
