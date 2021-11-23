# Controllers

You need to create controllers that inherit from `JsonApiController<TResource, TId>`

```c#
public class ArticlesController : JsonApiController<Article, Guid>
{
    public ArticlesController(IJsonApiOptions options, IResourceGraph resourceGraph,
        ILoggerFactory loggerFactory, IResourceService<Article, Guid> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
```

If you want to setup routes yourself, you can instead inherit from `BaseJsonApiController<TResource, TId>` and override its methods with your own `[HttpGet]`, `[HttpHead]`, `[HttpPost]`, `[HttpPatch]` and `[HttpDelete]` attributes added on them. Don't forget to add `[FromBody]` on parameters where needed.

## Resource Access Control

It is often desirable to limit which routes are exposed on your controller.

To provide read-only access, inherit from `JsonApiQueryController` instead, which blocks all POST, PATCH and DELETE requests.
Likewise, to provide write-only access, inherit from `JsonApiCommandController`, which blocks all GET and HEAD requests.

You can even make your own mix of allowed routes by calling the alternate constructor of `JsonApiController` and injecting the set of service implementations available.
In some cases, resources may be an aggregation of entities or a view on top of the underlying entities. In these cases, there may not be a writable `IResourceService` implementation, so simply inject the implementation that is available.

```c#
public class ReportsController : JsonApiController<Report, int>
{
    public ReportsController(IJsonApiOptions options, IResourceGraph resourceGraph,
        ILoggerFactory loggerFactory, IGetAllService<Report, int> getAllService)
        : base(options, resourceGraph, loggerFactory, getAll: getAllService)
    {
    }
}
```

For more information about resource service injection, see [Replacing injected services](~/usage/extensibility/layer-overview.md#replacing-injected-services) and [Resource Services](~/usage/extensibility/services.md).

When a route is blocked, an HTTP 403 Forbidden response is returned.

```http
DELETE http://localhost:14140/people/1 HTTP/1.1
```

```json
{
  "links": {
    "self": "/api/v1/people"
  },
  "errors": [
    {
      "id": "dde7f219-2274-4473-97ef-baac3e7c1487",
      "status": "403",
      "title": "The requested endpoint is not accessible.",
      "detail": "Endpoint '/people/1' is not accessible for DELETE requests."
    }
  ]
}
```
