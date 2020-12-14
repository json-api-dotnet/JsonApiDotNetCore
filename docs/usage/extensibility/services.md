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
        IResourceRepositoryAccessor repositoryAccessor,
        IQueryLayerComposer queryLayerComposer,
        IPaginationContext paginationContext,
        IJsonApiOptions options,
        ILoggerFactory loggerFactory,
        IJsonApiRequest request,
        IResourceChangeTracker<TodoItem> resourceChangeTracker,
        IResourceHookExecutorFacade hookExecutor)
        : base(repositoryAccessor, queryLayerComposer, paginationContext, options, loggerFactory,
            request, resourceChangeTracker, hookExecutor)
    {
        _notificationService = notificationService;
    }

    public override async Task<TodoItem> CreateAsync(TodoItem resource, CancellationToken cancellationToken)
    {
        // Call the base implementation
        var newResource = await base.CreateAsync(resource, cancellationToken);

        // Custom code
        await _notificationService.NotifyAsync($"Resource created: {newResource.StringId}");

        return newResource;
    }
}
```

## Not Using Entity Framework Core?

As previously discussed, this library uses Entity Framework Core by default.
If you'd like to use another ORM that does not provide what JsonApiResourceService depends upon, you can use a custom `IResourceService<TResource>` implementation.

```c#
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // add the service override for Product
    services.AddScoped<IResourceService<Product>, ProductService>();

    // add your own Data Access Object
    services.AddScoped<IProductDao, ProductDao>();
}

// ProductService.cs
public class ProductService : IResourceService<Product>
{
    private readonly IProductDao _dao;

    public ProductService(IProductDao dao)
    {
        _dao = dao;
    }

    public async Task<IReadOnlyCollection<Product>> GetAsync(CancellationToken cancellationToken)
    {
        return await _dao.GetProductsAsync(cancellationToken);
    }
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

In order to take advantage of these interfaces you first need to register the service for each implemented interface.

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

In v3.0 we introduced an extension method that you can use to register a resource service on all of its JsonApiDotNetCore interfaces.
This is helpful when you implement a subset of the resource interfaces and want to register them all in one go.

Note: If you're using service discovery, this happens automatically.

```c#
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddResourceService<ArticleService>();
    }
}
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
    public override async Task<IActionResult> PostAsync([FromBody] Article resource, CancellationToken cancellationToken)
    {
        return await base.PostAsync(resource, cancellationToken);
    }

    [HttpDelete("{id}")]
    public override async Task<IActionResult>DeleteAsync(int id, CancellationToken cancellationToken)
    {
        return await base.DeleteAsync(id, cancellationToken);
    }
}
```
