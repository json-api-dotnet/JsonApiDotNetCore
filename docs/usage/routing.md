# Routing

## Namespacing and Versioning URLs
You can add a namespace to all URLs by specifying it in ConfigureServices

```c#
public void ConfigureServices(IServiceCollection services)
{
  services.AddJsonApi<AppDbContext>(
      options => options.Namespace = "api/v1");
}
```
Which results in URLs like: https://yourdomain.com/api/v1/people

## Customizing Routes

The library will configure routes for each controller. By default, based on the [recommendations](https://jsonapi.org/recommendations/) outlined in the json:api spec, routes are camel-cased.

```http
GET /api/compoundModels HTTP/1.1
```

1. **Using the public name of the resource associated to a controller**.

```c#
public class MyResourceController : JsonApiController<MyApiResource> { /* .... */ }
```
Note that the route
- is `/myApiResources`, which matches the public resouce name in the json:api payload (`{ "type": "myApiResources", ... }`) 
- can be configured by configuring the public resource name. See [this section](~/usage/resource-graph.md#public-resource-name) on how to do that. 


2. **Using the controller name**. 
If a controller does not have an associated resource, the name of the controller will be used following the configured naming strategy.
```c#
public class MyResourceController : ControllerBase { /* .... */ }
```
Note that the route is `myResources` can be changed by renaming the controller. This approach is the default .NET Core MVC routing approach.

## Customizing the Routing Convention
It is possible to fully customize routing behavior by registering a `IJsonApiRoutingConvention` implementation.
```c#
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IJsonApiConvention, CustomRoutingConvention>();
}
```

## Disabling the Default Routing Convention
It is possible to completely bypass the default routing convention for a particular controller and specify a custom routing template by using the `DisableRoutingConvention` attribute.
In the following example, the `CamelCasedModel` resource can be accessed on `/my-custom-route`.

```c#
[Route("my-custom-route"), DisableRoutingConvention]
public class MyCustomResourceController : JsonApiController<CamelCasedModel> { /* ... */ }
```
