# Controllers

You need to create controllers that inherit from `JsonApiController<TResource>`

```c#
public class ArticlesController : JsonApiController<Article>
{
    public ArticlesController(
        IJsonApiOptions options,
        ILoggerFactory loggerFactory,
        IResourceService<Article> resourceService)
        : base(options, loggerFactory, resourceService)
    { }
}
```

## Non-Integer Type Keys

If your model is using a type other than `int` for the primary key, you must explicitly declare it in the controller/service/repository definitions.

```c#
public class ArticlesController : JsonApiController<Article, Guid>
//---------------------------------------------------------- ^^^^
{
    public ArticlesController(
        IJsonApiOptions options,
        ILoggerFactory loggerFactory,
        IResourceService<Article, Guid> resourceService)
        //----------------------- ^^^^
        : base(options, loggerFactory, resourceService)
    { }
}
```

## Resource Access Control

It is often desirable to limit what methods are exposed on your controller. The first way you can do this, is to simply inherit from `BaseJsonApiController` and explicitly declare what methods are available.

In this example, if a client attempts to do anything other than GET a resource, an HTTP 404 Not Found response will be returned since no other methods are exposed.

This approach is ok, but introduces some boilerplate that can easily be avoided.

```c#
public class ArticlesController : BaseJsonApiController<Article>
{
    public ArticlesController(
        IJsonApiOptions options,
        ILoggerFactory loggerFactory,
        IResourceService<Article> resourceService)
        : base(options, loggerFactory, resourceService)
    { }

    [HttpGet]
    public override async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        return await base.GetAsync(cancellationToken);
    }

    [HttpGet("{id}")]
    public override async Task<IActionResult> GetAsync(int id, CancellationToken cancellationToken)
    {
        return await base.GetAsync(id, cancellationToken);
    }
}
```

## Using ActionFilterAttributes

The next option is to use the ActionFilter attributes that ship with the library. The available attributes are:

- `NoHttpPost`: disallow POST requests
- `NoHttpPatch`: disallow PATCH requests
- `NoHttpDelete`: disallow DELETE requests
- `HttpReadOnly`: all of the above

Not only does this reduce boilerplate, but it also provides a more meaningful HTTP response code.
An attempt to use one blacklisted methods will result in a HTTP 405 Method Not Allowed response.

```c#
[HttpReadOnly]
public class ArticlesController : BaseJsonApiController<Article>
{
    public ArticlesController(
        IJsonApiOptions options,
        ILoggerFactory loggerFactory,
        IResourceService<Article> resourceService)
        : base(options, loggerFactory, resourceService)
    { }
}
```

## Implicit Access By Service Injection

Finally, you can control the allowed methods by supplying only the available service implementations. In some cases, resources may be an aggregation of entities or a view on top of the underlying entities. In these cases, there may not be a writable IResourceService implementation, so simply inject the implementation that is available.

As with the ActionFilter attributes, if a service implementation is not available to service a request, HTTP 405 Method Not Allowed will be returned.

For more information about resource injection, see the next section titled Resource Services.

```c#
public class ReportsController : BaseJsonApiController<Report>
{
    public ReportsController(
        IJsonApiOptions options,
        ILoggerFactory loggerFactory,
        IResourceService<Report> resourceService)
        : base(options, loggerFactory, resourceService)
    { }

    [HttpGet]
    public override async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        return await base.GetAsync(cancellationToken);
    }
}
```
