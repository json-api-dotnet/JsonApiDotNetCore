# The Resource Graph

The `ResourceGraph` is a map of all the JSON:API resources and their relationships that your API serves.
It is built at app startup and available as a singleton through Dependency Injection.

> [!NOTE]
> Prior to v4, this was called the `ContextGraph`.

## Constructing The Graph

There are three ways the resource graph can be created:

1. Auto-discovery
2. Specifying an entire DbContext
3. Manually specifying each resource

It is also possible to combine the three of them at once. Be aware that some configuration might overlap,
for example one could manually add a resource to the graph which is also auto-discovered. In such a scenario, the configuration
is prioritized by the list above in descending order.

### Auto-discovery

Auto-discovery refers to the process of reflecting on an assembly and
detecting all of the JSON:API resources, resource definitions, resource services and repositories.

The following command builds the resource graph using all `IIdentifiable` implementations and registers the services mentioned.
You can enable auto-discovery for the current assembly by adding the following at startup.

```c#
// Program.cs
builder.Services.AddJsonApi(discovery: discovery => discovery.AddCurrentAssembly());
```

### Specifying an Entity Framework Core DbContext

If you are using Entity Framework Core as your ORM, you can add all the models of a `DbContext` to the resource graph.

```c#
// Program.cs
builder.Services.AddJsonApi<AppDbContext>();
```

Be aware that this does not register resource definitions, resource services and repositories. You can combine it with auto-discovery to achieve this.

```c#
// Program.cs
builder.Services.AddJsonApi<AppDbContext>(discovery: discovery => discovery.AddCurrentAssembly());
```

### Manual Specification

You can manually construct the graph.

```c#
// Program.cs
builder.Services.AddJsonApi(resources: resourceGraphBuilder =>
    resourceGraphBuilder.Add<Person, int>());
```

## Resource Name

The public resource name is exposed through the `type` member in the JSON:API payload. This can be configured using the following approaches (in order of priority):

1. The `publicName` parameter when manually adding a resource to the graph.
```c#
// Program.cs
builder.Services.AddJsonApi(resources: resourceGraphBuilder =>
{
    resourceGraphBuilder.Add<Person, int>(publicName: "individuals");
});
```

2. The `PublicName` property when a model is decorated with a `ResourceAttribute`.
```c#
[Resource(PublicName = "individuals")]
public class Person : Identifiable<int>
{
}
```

3. The configured naming convention (by default this is camel-case), after pluralization.
```c#
// this will be registered as "people"
public class Person : Identifiable<int>
{
}
```

The default naming convention can be changed in [options](~/usage/options.md#customize-serializer-options).
