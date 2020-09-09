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

The library will configure routes for all controllers in your project. By default, routes are camel-cased. This is based on the [recommendations](https://jsonapi.org/recommendations/) outlined in the json:api spec. The public name of the resource ([which can be customized](./resource-graph.md#public-resource-name)) is used for the route.

```c#
public class OrderLine : Identifiable {  }

public class OrderController : JsonApiController<OrderLine> { /* .... */ }
```

```http
GET /orderLines HTTP/1.1
```

### Non-json:api controllers

If a controller is not associated to a resource, the [configured naming strategy](./options.md#custom-serializer-settings) will be applied to the name of the controller.
```c#
public class OrderLineController : ControllerBase { /* .... */ }
```
```http
GET /orderLines HTTP/1.1
```

## Disabling the Default Routing Convention

It is possible to by-pass the default routing convention for a controller.
```c#
[Route("v1/camelCasedModels"), DisableRoutingConvention]
public class MyCustomResourceController : JsonApiController<CamelCasedModel> { /* ... */ }
```
In order to have valid link building, it is required to match your custom url with the public name of the associated resource.

## Advanced Usage: Custom Routing Convention.

It is possible to register a [custom routing convention]](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/application-model?view=aspnetcore-3.1#sample-custom-routing-convention) by registering an implementation of `IJsonApiRoutingConvention`. This is generally not recommended and for advanced usage only.
```c#
public void ConfigureServices(IServiceCollection services)
{
	services.AddSingleton<IJsonApiConvention, CustomRoutingConvention>();
}
```