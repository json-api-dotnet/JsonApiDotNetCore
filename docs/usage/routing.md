# Routing

## Namespacing and Versioning URLs
You can add a namespace to all URLs by specifying it in ConfigureServices.

```c#
public void ConfigureServices(IServiceCollection services)
{
  services.AddJsonApi<AppDbContext>(
      options => options.Namespace = "api/v1");
}
```
Which results in URLs like: https://yourdomain.com/api/v1/people

## Default Routing Convention

The library will configure routes for all controllers in your project. By default, routes are camel-cased. This is based on the [recommendations](https://jsonapi.org/recommendations/) outlined in the json:api spec.

```c#
public class OrderLine : Identifiable { }

public class OrderLineController : JsonApiController<OrderLine>
{
    public OrderLineController(IJsonApiOptions options, ILoggerFactory loggerFactory,
        IResourceService<OrderLine> resourceService)
        : base(options, loggerFactory, resourceService)
    {
    }
}
```

```http
GET /orderLines HTTP/1.1
```

The exposed name of the resource ([which can be customized](~/usage/resource-graph.md#resource-name)) is used for the route, instead of the controller name.

### Non-json:api controllers

If a controller does not inherit from `JsonApiController<TResource>`, the [configured naming convention](~/usage/options.md#custom-serializer-settings) is applied to the name of the controller.
```c#
public class OrderLineController : ControllerBase { }
```
```http
GET /orderLines HTTP/1.1
```

## Disabling the Default Routing Convention

It is possible to bypass the default routing convention for a controller.
```c#
[Route("v1/custom/route/orderLines"), DisableRoutingConvention]
public class OrderLineController : JsonApiController<OrderLine>
{
    public OrderLineController(IJsonApiOptions options, ILoggerFactory loggerFactory,
        IResourceService<OrderLine> resourceService)
        : base(options, loggerFactory, resourceService)
    {
    }
}
```
It is required to match your custom url with the exposed name of the associated resource.

## Advanced Usage: Custom Routing Convention

It is possible to replace the built-in routing convention with a [custom routing convention](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/application-model?view=aspnetcore-3.1#sample-custom-routing-convention) by registering an implementation of `IJsonApiRoutingConvention`.
```c#
public void ConfigureServices(IServiceCollection services)
{
	services.AddSingleton<IJsonApiRoutingConvention, CustomRoutingConvention>();
}
```