# Routing
The library will configure routes for each controller. By default, based on the [recommendations](https://jsonapi.org/recommendations/) outlined in the json:api spec, routes are camel-cased.

```http
GET /api/compoundModels HTTP/1.1
```

There are two ways the library will try to create a route for a controller:
1. **By inspecting the controller for an associated resource**. The library will try to first use the public resource name of the resource associated to a controller. This means that the value of the `type` member of the json:api document for a resource will be equal to the route.
Note that this implies that it is possible to configure a route configuring the exposed resource name. See [this section](~/usage/resource-graph.md#public-resource-name) on how this can be achieved. Example:
```c#
// controller
public class MyResourceController : JsonApiController<MyApiResource> { /* .... */ } // note that the route is NOT "myResources", but "myApiResources"

// request
GET /myApiResources HTTP/1.1

// response
HTTP/1.1 200 OK
Content-Type: application/vnd.api+json

{
  "data": [{
    "type": "myApiResources",
    "id": "1",
    "attributes": { ... }
  }]
}
```
2. **By using the name of the controller**. If no associated resource was detected for a controller, the library will construct a route from the name of the controller by using the configured naming strategy (*camelCase* by default, see [this section](~/usage/resource-graph.md#public-resource-name) on how to configure this). This is in alignment with the default .NET Core MVC routing approach. 
In the following example the controller is not associated to a resource by the library because it does not inherit from `BaseJsonApiController<T>`.
```c#
// controller
public class MyResourceController : ControllerBase { /* .... */ }

// request
GET /myResources HTTP/1.1
```

## Customized the Routing Convention
It is possible to fully customize routing behaviour by registering a `IJsonApiRoutingConvention` implementation **before** calling `AddJsonApi( ... )`.
```c#
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
<<<<<<< HEAD
    services.AddSingleton<IJsonApiConvention, CustomRoutingConvention>();
    services.AddJsonApi( /* ... */ );
=======
    public CamelCasedModelsController(
        IJsonApiOptions options,
        ILoggerFactory loggerFactory,
        IResourceService<CamelCasedModel> resourceService)
        : base(options, loggerFactory, resourceService)
    { }
>>>>>>> master
}
```

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

## Disabling the Default Routing Convention
It is possible to completely bypass the default routing convention for a particular controller and specify a custom routing template by using the `DisableRoutingConvention` attribute.
In the following example, the `CamelCasedModel` resource can be accessed on `/my-custom-route`.

```c#
[Route("my-custom-route"), DisableRoutingConvention]
public class MyCustomResourceController : JsonApiController<CamelCasedModel> { /* ... */ }
```
