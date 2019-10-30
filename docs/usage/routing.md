# Routing

By default the library will configure routes for each controller. 
Based on the recommendations outlined in the JSONAPI spec, routes are hyphenated.

```http
GET /api/compound-models HTTP/1.1
Accept: application/vnd.api+json
```

## Namespacing and Versioning URLs

You can add a namespace to the URL by specifying it in ConfigureServices

```c#
public IServiceProvider ConfigureServices(IServiceCollection services) {
  services.AddJsonApi<AppDbContext>(
      opt => opt.Namespace = "api/v1");
}
```

## Disable Convention

You can disable the dasherized convention and specify your own template by using the `DisableRoutingConvention` Attribute.

```c#
[Route("[controller]")]
[DisableRoutingConvention]
public class CamelCasedModelsController : JsonApiController<CamelCasedModel> {
    public CamelCasedModelsController(
        IJsonApiContext jsonApiContext,
        IResourceService<CamelCasedModel> resourceService) 
        : base(jsonApiContext, resourceService)
    { }
}
```

It is important to note that your routes must still end with the model name in the same format as the resource name. This is so that we can build accurate resource links in the json:api document. For example, if you define a resource as MyModels the controller route must match.

```c#
public IServiceProvider ConfigureServices(IServiceCollection services) {
  services.AddJsonApi(options => {
      options.BuildContextGraph((builder) => {
          builder.AddResource<TodoItem>("myModels"); // camelCased
      });
  });
}

// controller definition
[Route("api/myModels"), DisableRoutingConvention]
public class MyModelsController : JsonApiController<TodoItem> { 
  //...
}
```

See [this](~/usage/resource-graph.html#public-resource-type-name) for 
more information on how the resource name is determined.