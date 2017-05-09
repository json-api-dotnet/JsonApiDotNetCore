---
currentMenu: routing
---

# Routing

By default the library will configure routes for each controller. 
Based on the [recommendations](http://jsonapi.org/recommendations/)
outlined in the JSONAPI spec, routes are hyphenated. For example:

```
/todo-items --> TodoItemsController
NOT /todoItems
```

## Namespacing and Versioning URLs

You can add a namespace to the URL by specifying it in `ConfigureServices`:

```csharp
services.AddJsonApi<AppDbContext>(
    opt => opt.Namespace = "api/v1");
```

## Disable Convention

You can disable the dasherized convention and specify your own template
by using the `DisableRoutingConvention` Attribute. 

```csharp
[Route("[controller]")]
[DisableRoutingConvention]
public class CamelCasedModelsController : JsonApiController<CamelCasedModel>
{
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
// resource definition
builder.AddResource<TodoItem>("myModels");

// controller definition
[Route("api/myModels")]
[DisableRoutingConvention]
public class TodoItemsController : JsonApiController<TodoItem>
{ //...
}
```
