---
currentMenu: contextgraph
---

# Context Graph

The [ContextGraph](https://github.com/Research-Institute/json-api-dotnet-core/blob/master/src/JsonApiDotNetCore/Internal/ContextGraph.cs) is a map of all the json:api resources and their relationships that your API serves.
It is built at app startup and available as a singleton through Dependency Injection. 

When you call `services.AddJsonApi<AppDbContext>()`, the graph is constructed from the context.

### Defining Non-EF Resources

If you have models that are not members of a `DbContext`, 
you can manually create this graph like so:

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

### Changing Resource Names

If a DbContext is specified when adding the services, the context will be used to define the resources and their names. By default, these names will be hyphenated.

```csharp
public class AppDbContext : DbContext {
    // this will be translated into "my-models"
    public DbSet<MyModel> MyModels { get; set; }
}
```

However, you can specify a custom name like so:

```csharp
public class AppDbContext : DbContext {
    // this will be translated into "someModels"
    [Resource("someModels")]
    public DbSet<MyModel> MyModels { get; set; }
}
```





