# Resource Services

The `IResourceService` acts as a service layer between the controller and the data access layer.
This allows you to customize it however you want. This is also a good place to implement custom business logic.

## Supplementing Default Behavior

If you don't need to alter the underlying mechanisms, you can inherit from `JsonApiResourceService<TResource>` and override the existing methods.
In simple cases, you can also just wrap the base implementation with your custom logic.

A simple example would be to send notifications when a resource gets created.

```c#
public class TodoItemService : JsonApiResourceService<TodoItem>
{
    private readonly INotificationService _notificationService;

    public TodoItemService(
        IResourceRepository<TResource, int> repository,
        IQueryLayerComposer queryLayerComposer,
        IPaginationContext paginationContext,
        IJsonApiOptions options,
        ILoggerFactory loggerFactory,
        ICurrentRequest currentRequest,
        IResourceChangeTracker<TResource> resourceChangeTracker,
        IResourceFactory resourceFactory,
        IResourceHookExecutor hookExecutor = null)
        : base(repository, queryLayerComposer, paginationContext, options, loggerFactory, currentRequest,
            resourceChangeTracker, resourceFactory, hookExecutor)
    {
        _notificationService = notificationService;
    }

    public override async Task<TodoItem> CreateAsync(TodoItem resource)
    {
        // Call the base implementation
        var newResource = await base.CreateAsync(resource);

        // Custom code
        _notificationService.Notify($"Resource created: {newResource.StringId}");

        return newResource;
    }
}
```

## Not Using Entity Framework Core?

As previously discussed, this library uses Entity Framework Core by default.
If you'd like to use another ORM that does not provide what JsonApiResourceService depends upon, you can use a custom `IResourceService<T>` implementation.

```c#
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // add the service override for MyModel
    services.AddScoped<IResourceService<MyModel>, MyModelService>();

    // add your own Data Access Object
    services.AddScoped<IMyModelDao, MyModelDao>();
    // ...
}

// MyModelService.cs
public class MyModelService : IResourceService<MyModel>
{
    private readonly IMyModelDao _dao;

    public MyModelService(IMyModelDao dao)
    {
        _dao = dao;
    }

    public Task<IEnumerable<MyModel>> GetAsync()
    {
        return await _dao.GetModelAsync();
    }

    // ...
}
```

## Limited Requirements

In some cases it may be necessary to only expose a few methods on a resource. For this reason, we have created a hierarchy of service interfaces that can be used to get the exact implementation you require.

This interface hierarchy is defined by this tree structure.

```
IResourceService
|
+-- IResourceQueryService
|   |
|   +-- IGetAllService
|   |   GET /
|   |
|   +-- IGetByIdService
|   |   GET /{id}
|   |
|   +-- IGetSecondaryService
|   |   GET /{id}/{relationship}
|   |
|   +-- IGetRelationshipService
|       GET /{id}/relationships/{relationship}
|
+-- IResourceCommandService
    |
    +-- ICreateService
    |   POST /
    |
    +-- IDeleteService
    |   DELETE /{id}
    |
    +-- IUpdateService
    |   PATCH /{id}
    |
    +-- IUpdateRelationshipService
        PATCH /{id}/relationships/{relationship}
```

In order to take advantage of these interfaces you first need to inject the service for each implemented interface.

```c#
public class ArticleService : ICreateService<Article>, IDeleteService<Article>
{
    // ...
}

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ICreateService<Article>, ArticleService>();
        services.AddScoped<IDeleteService<Article>, ArticleService>();
    }
}
```

Other dependency injection frameworks such as Autofac can be used to simplify this syntax.

```c#
builder.RegisterType<ArticleService>().AsImplementedInterfaces();
```

Then in the controller, you should inherit from the base controller and pass the services into the named, optional base parameters:

```c#
public class ArticlesController : BaseJsonApiController<Article>
{
    public ArticlesController(
        IJsonApiOptions options,
        ILoggerFactory loggerFactory,
        ICreateService<Article, int> create,
        IDeleteService<Article, int> delete)
        : base(options, loggerFactory, create: create, delete: delete)
    { }

    [HttpPost]
    public override async Task<IActionResult> PostAsync([FromBody] Article resource)
    {
        return await base.PostAsync(resource);
    }

    [HttpDelete("{id}")]
    public override async Task<IActionResult>DeleteAsync(int id)
    {
        return await base.DeleteAsync(id);
    }
}
```
