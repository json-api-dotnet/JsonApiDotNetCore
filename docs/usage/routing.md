# Routing

An endpoint URL provides access to a resource or a relationship. Resource endpoints are divided into:
- Primary endpoints, for example: "/articles" and "/articles/1".
- Secondary endpoints, for example: "/articles/1/author" and "/articles/1/comments".

In the relationship endpoint "/articles/1/relationships/comments", "articles" is the left side of the relationship and "comments" the right side.

## Namespacing and versioning of URLs
You can add a namespace to all URLs by specifying it at startup.

```c#
// Program.cs
builder.Services.AddJsonApi<AppDbContext>(options => options.Namespace = "api/v1");
```

Which results in URLs like: https://yourdomain.com/api/v1/people

## Default routing convention

The library will configure routes for all auto-generated and hand-written controllers in your project. By default, routes are camel-cased. This is based on the [recommendations](https://jsonapi.org/recommendations/) outlined in the JSON:API spec.

```c#
// Auto-generated
[Resource]
public class OrderSummary : Identifiable<int>
{
}

// Hand-written
public class OrderLineController : JsonApiController<OrderLine, int>
{
    public OrderLineController(IJsonApiOptions options, IResourceGraph resourceGraph,
        ILoggerFactory loggerFactory, IResourceService<OrderLine, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
```

```http
GET /orderSummaries HTTP/1.1
GET /orderLines HTTP/1.1
```

The exposed name of the resource ([which can be customized](~/usage/resource-graph.md#resource-name)) is used for the route, instead of the controller name.

### Non-JSON:API controllers

If a controller does not inherit from `JsonApiController<TResource, TId>`, the [configured naming convention](~/usage/options.md#customize-serializer-options) is applied to the name of the controller.

```c#
public class OrderLineController : ControllerBase
{
}
```

```http
GET /orderLines HTTP/1.1
```

### Customized routes

It is possible to override the default routing convention for an auto-generated or hand-written controller.

```c#
// Auto-generated
[DisableRoutingConvention]
[Route("v1/custom/route/summaries-for-orders")]
partial class OrderSummariesController
{
}

// Hand-written
[DisableRoutingConvention]
[Route("v1/custom/route/lines-in-order")]
public class OrderLineController : JsonApiController<OrderLine, int>
{
    public OrderLineController(IJsonApiOptions options, IResourceGraph resourceGraph,
        ILoggerFactory loggerFactory, IResourceService<OrderLine, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
```

## Advanced usage: custom routing convention

It is possible to replace the built-in routing convention with a [custom routing convention](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/application-model?view=aspnetcore-3.1#sample-custom-routing-convention) by registering an implementation of `IJsonApiRoutingConvention`.

```c#
// Program.cs
builder.Services.AddSingleton<IJsonApiRoutingConvention, CustomRoutingConvention>();
```
