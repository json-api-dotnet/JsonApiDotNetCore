# Routing

An endpoint URL provides access to a resource or a relationship. Resource endpoints are divided into:
- Primary endpoints, for example: "/articles" and "/articles/1".
- Secondary endpoints, for example: "/articles/1/author" and "/articles/1/comments".

In the relationship endpoint "/articles/1/relationships/comments", "articles" is the left side of the relationship and "comments" the right side.

## Namespacing and Versioning URLs
You can add a namespace to all URLs by specifying it in ConfigureServices.

```c#
public void ConfigureServices(IServiceCollection services)
{
  services.AddJsonApi<AppDbContext>(options => options.Namespace = "api/v1");
}
```

Which results in URLs like: https://yourdomain.com/api/v1/people

## Default Routing Convention

The library will configure routes for all controllers in your project. By default, routes are camel-cased. This is based on the [recommendations](https://jsonapi.org/recommendations/) outlined in the JSON:API spec.

```c#
public class OrderLine : Identifiable<int>
{
}

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

## Disabling the Default Routing Convention

It is possible to bypass the default routing convention for a controller.

```c#
[Route("v1/custom/route/lines-in-order"), DisableRoutingConvention]
public class OrderLineController : JsonApiController<OrderLine, int>
{
    public OrderLineController(IJsonApiOptions options, IResourceGraph resourceGraph,
        ILoggerFactory loggerFactory, IResourceService<OrderLine, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
```

## Advanced Usage: Custom Routing Convention

It is possible to replace the built-in routing convention with a [custom routing convention](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/application-model?view=aspnetcore-3.1#sample-custom-routing-convention) by registering an implementation of `IJsonApiRoutingConvention`.

```c#
public void ConfigureServices(IServiceCollection services)
{
	services.AddSingleton<IJsonApiRoutingConvention, CustomRoutingConvention>();
}
```
