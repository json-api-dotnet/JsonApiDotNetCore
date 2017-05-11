---
currentMenu: routing
---

# Routing

By default the library will configure routes for each controller. 
Based on the [recommendations](http://jsonapi.org/recommendations/)
outlined in the JSONAPI spec, routes are hyphenated.

```http
GET /api/compound-models HTTP/1.1
Accept: application/vnd.api+json
```

## Namespacing and Versioning URLs

You can add a namespace to the URL by specifying it in `ConfigureServices`:

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services) {
    services.AddJsonApi<AppDbContext>(
        opt => opt.Namespace = "api/v1");
}
```

## Disable Convention

You can disable the dasherized convention and specify your own template
by using the `DisableRoutingConvention` Attribute. 

```csharp
[Route("[controller]")]
[DisableRoutingConvention]
public class CamelCasedModelsController : JsonApiController<CamelCasedModel> {
    public CamelCasedModelsController(
        IJsonApiContext jsonApiContext,
        IResourceService<CamelCasedModel> resourceService,
        ILoggerFactory loggerFactory) 
        : base(jsonApiContext, resourceService, loggerFactory)
    { }
}
```

It is important to note that your routes *must* still end with the model name in the same format
as the resource name. This is so that we can build accurrate resource links in the json:api document.
For example, if you define a resource as `MyModels` the controller route must match:

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services) {
    services.AddJsonApi(options => {
        options.BuildContextGraph((builder) => {
            // resource definition
            builder.AddResource<TodoItem>("myModels");
        });
    });
}

// controller definition
[Route("api/myModels")]
[DisableRoutingConvention]
public class TodoItemsController : JsonApiController<TodoItem> { 
    //...
}
```
