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

## Customizing Routes

The library will configure routes for all controllers in your project.

### Json:api endpoints

By default, for json:api controllers, 
- routes are camel-cased. This is based on the [recommendations](https://jsonapi.org/recommendations/) outlined in the json:api spec.
- the route of a controller will match the public name of the resource that is associated it. This means that routes can be customized by [configuring the public name of the associated resource](./resource-graph.md#public-resource-name).

```c#
public class MyResourceController : JsonApiController<MyApiResource> { /* .... */ }
```
The route for this example will be `/myApiResources`, which will match the type in the json:api payload: `{ "type": "myApiResources", ... }`.


### Non-json:api endpoints

If a controller does not have an associated resource, the [configured naming strategy](./options#custom-serializer-settings) will be applied to the name of the controller.
```c#
public class MyResourceController : ControllerBase { /* .... */ }
```
The route for this example is `/myResources`, which can be changed by renaming the controller.


## Disabling the Default Routing Convention

It is possible to by-pass the default routing convention for a controller by combining the `Route` and `DisableRoutingConvention`attributes. Any usage of `Route` without `DisableRoutingConvention` is ignored.
```c#
[Route("v1/camelCasedModels"), DisableRoutingConvention]
public class MyCustomResourceController : JsonApiController<CamelCasedModel> { /* ... */ }
```
This example exposes a versioned `CamelCasedModel` endpoint. To ensure guarantee valid link building, it is *highly recommended* to match your custom url with the public name of the associated resource.

## Advanced Usage: Custom Routing Convention.

It is possible to use a [custom routing convention](add-link) by registering a custom `IJsonApiRoutingConvention` implementation. This is generally not recommended and for advanced usage only.
```c#
public void ConfigureServices(IServiceCollection services)
{
	services.AddSingleton<IJsonApiConvention, CustomRoutingConvention>();
}
```