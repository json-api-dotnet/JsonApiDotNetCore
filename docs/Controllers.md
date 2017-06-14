---
currentMenu: controllers
---

# Controllers

You need to create controllers that inherit from [JsonApiController&lt;TEntity&gt;](https://github.com/Research-Institute/json-api-dotnet-core/blob/master/src/JsonApiDotNetCore/Controllers/JsonApiController.cs).

```csharp
public class ThingsController : JsonApiController<Thing>
{
    public ThingsController(
        IJsonApiContext jsonApiContext,
        IResourceService<Thing> resourceService,
        ILoggerFactory loggerFactory) 
    : base(jsonApiContext, resourceService, loggerFactory)
    { }
}
```

### Non-Integer Type Keys

If your model is using a type other than `int` for the primary key,
you should explicitly declare it in the controller
and service generic type definitions:

```csharp
public class ThingsController : JsonApiController<Thing, Guid>
                                //---------------------- ^^^^
{
    public ThingsController(
        IJsonApiContext jsonApiContext,
        IResourceService<Thing, Guid> resourceService,
        //--------------------- ^^^^
        ILoggerFactory loggerFactory) 
    : base(jsonApiContext, resourceService, loggerFactory)
    { }
}
```

### Limiting Write Access

It is possible to limit write resource access on the controller entirely using the following attributes:

- `NoHttpPost`: disallow POST requests
- `NoHttpPatch`: disallow PATCH requests
- `NoHttpDelete`: disallow DELETE requests
- `HttpReadOnly`: all of the above

```csharp
[HttpReadOnly]
public class ThingsController : JsonApiController<Thing>
{
    public ThingsController(
        IJsonApiContext jsonApiContext,
        IResourceService<Thing> resourceService,
        ILoggerFactory loggerFactory) 
    : base(jsonApiContext, resourceService, loggerFactory)
    { }
}
```

### Additional customizations

If you need additional customization at the controller level, you can override the virtual
methods. Please be aware that this is not the place for advanced business logic
which should be performed at the [service](resourceservices.html) or [repository](entityrepositories.html) layers. Here is an example override at the controller layer:

```csharp
public class TodoItemsController : JsonApiController<TodoItem>
{
    public TodoItemsController(
        IJsonApiContext jsonApiContext,
        IResourceService<TodoItem> resourceService,
        ILoggerFactory loggerFactory) 
        : base(jsonApiContext, resourceService, loggerFactory)
    { }

    [HttpGet]
    public override async Task<IActionResult> GetAsync()
    {
        // custom code
        if(RequestIsValid() == false)
            return BadRequest();
        
        // return result from base class
        return await base.GetAsync();
    }

    // some custom validation logic
    private bool RequestIsValid() => true;
}
```
