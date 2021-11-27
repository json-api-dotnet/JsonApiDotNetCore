# Controllers

To expose API endpoints, ASP.NET controllers need to be defined.

_since v5_

Controllers are auto-generated (using [source generators](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview)) when you add `[Resource]` on your model class:

```c#
[Resource] // Generates ArticlesController.g.cs
public class Article : Identifiable<Guid>
{
    // ...
}
```

## Resource Access Control

It is often desirable to limit which endpoints are exposed on your controller.
A subset can be specified too:

```c#
[Resource(GenerateControllerEndpoints =
    JsonApiEndpoints.GetCollection | JsonApiEndpoints.GetSingle)]
public class Article : Identifiable<Guid>
{
    // ...
}
```

Instead of passing a set of endpoints, you can use `JsonApiEndpoints.Query` to generate all read-only endpoints or `JsonApiEndpoints.Command` for all write-only endpoints.

When an endpoint is blocked, an HTTP 403 Forbidden response is returned.

```http
DELETE http://localhost:14140/articles/1 HTTP/1.1
```

```json
{
  "links": {
    "self": "/articles"
  },
  "errors": [
    {
      "id": "dde7f219-2274-4473-97ef-baac3e7c1487",
      "status": "403",
      "title": "The requested endpoint is not accessible.",
      "detail": "Endpoint '/articles/1' is not accessible for DELETE requests."
    }
  ]
}
```

## Augmenting controllers

Auto-generated controllers can easily be augmented because they are partial classes. For example:

```c#
[DisableRoutingConvention]
[Route("some/custom/route")]
[DisableQueryString(JsonApiQueryStringParameters.Include)]
partial class ArticlesController
{
    [HttpPost]
    public IActionResult Upload()
    {
        // ...
    }
}
```

If you need to inject extra dependencies, tell the IoC container with `[ActivatorUtilitiesConstructor]` to prefer your constructor:

```c#
partial class ArticlesController
{
    private IAuthenticationService _authService;

    [ActivatorUtilitiesConstructor]
    public ArticlesController(IAuthenticationService authService, IJsonApiOptions options,
        IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Article, Guid> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
    {
        _authService = authService;
    }
}
```

In case you don't want to use auto-generated controllers and define them yourself (see below), remove
`[Resource]` from your models or use `[Resource(GenerateControllerEndpoints = JsonApiEndpoints.None)]`.

## Earlier versions

In earlier versions of JsonApiDotNetCore, you needed to create controllers that inherit from `JsonApiController<TResource, TId>`. For example:

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

### Resource Access Control

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
