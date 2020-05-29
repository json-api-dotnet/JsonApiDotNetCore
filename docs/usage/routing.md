# Routing

By default the library will configure routes for each controller.
Based on the [recommendations](https://jsonapi.org/recommendations/) outlined in the json:api spec, routes are camel-cased.

```http
GET /api/compoundModels HTTP/1.1
```

## Namespacing and Versioning URLs

You can add a namespace to all URLs by specifying it in ConfigureServices

```c#
public void ConfigureServices(IServiceCollection services)
{
  services.AddJsonApi<AppDbContext>(
      options => options.Namespace = "api/v1");
}
```
Which results in URLs like: https://yourdomain.com/api/v1/people

## Disable Convention

You can disable the default casing convention and specify your own template by using the `DisableRoutingConvention` attribute.

```c#
[Route("[controller]")]
[DisableRoutingConvention]
public class CamelCasedModelsController : JsonApiController<CamelCasedModel>
{
    public CamelCasedModelsController(
        IJsonApiOptions jsonApiOptions,
        ILoggerFactory loggerFactory,
        IResourceService<CamelCasedModel> resourceService)
        : base(jsonApiOptions, loggerFactory, resourceService)
    { }
}
```

It is important to note that your routes must still end with the model name in the same format as the resource name. This is so that we can build accurate resource links in the json:api document. For example, if you define a resource as MyModels, the controller route must match.

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddJsonApi(resources: builder =>
        builder.AddResource<TodoItem>("my-models")); // kebab-cased
}

// controller definition
[Route("api/my-models"), DisableRoutingConvention]
public class MyModelsController : JsonApiController<TodoItem>
{
  //...
}
```

See [this](~/usage/resource-graph.md#public-resource-type-name) for
more information on how the resource name is determined.
