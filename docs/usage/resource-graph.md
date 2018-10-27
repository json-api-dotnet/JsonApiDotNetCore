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

Auto-discovery refers to process of reflecting on an assembly and 
detecting all of the json:api resources and services.

The following command will build the context graph using all `IIdentifiable`
implementations. It also injects service layer overrides which we will 
cover in a later section. You can enable auto-discovery for the 
current assembly by adding the following to your `Startup` class.

```c#
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddJsonApi(
        options => { /* ... */ }, 
        mvcBuilder,
        discovery => discovery.AddCurrentAssembly());
}
```

### Entity Framework DbContext

If you are using Entity Framework Core as your ORM, you can add an entire `DbContext` with one line.

```c#
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddJsonApi<AppDbContext>();
}
```

### Manual Specification

You can also manually construct the graph.

```c#
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    var mvcBuilder = services.AddMvc();

    services.AddJsonApi(options => {
        options.BuildResourceGraph((builder) => {
            builder.AddResource<MyModel>();
        });
    }, mvcBuilder);
}
```

### Public Resource Type Name

The public resource type name for is determined by the following criteria (in order of priority):

1. The model is decorated with a `ResourceAttribute`
```c#
[Resource("my-models")]
public class MyModel : Identifiable { /* ... */ }
```

2. The `DbSet` is decorated with a `ResourceAttribute`. Note that this only applies if the graph was created from the DbContext (i.e. `services.AddJsonApi<AppDbContext>()`)
```c#
[Resource("my-models")] 
public DbSet<MyModel> MyModel { get; set; }
```

3. The configured naming convention (by default this is kebab-case).
```c#
// this will be registered as "my-models"
public class MyModel : Identifiable { /* ... */ }
```
