# Middleware

The following is the default configuration of JsonApiDotNetCore:
1. Call one of the `AddJsonApi( ... )` overloads in the ` Startup.ConfigureServices` method. In this example uses a `DbContext` to build the resource graph

```c#
services.AddJsonApi<AppDbContext>();
```

2. In the Startup.Configure method, configure your application to use routing, to add the JsonApiMiddleware and to configure endpoint routing.

```c#
app.UseRouting();
app.UseJsonApi();
app.UseEndpoints(endpoints => endpoints.MapControllers());
```

The following middleware components, in respective order, are registered:



Filters:
- `IJsonApiExceptionFilter`
- `IJsonApiTypeMatchFilter`
- `IQueryStringActionFilter`
- `IConvertEmptyActionResultFilter`

Formatters:
- `IJsonApiInputFormatter`
- `IJsonApiOutputFormatter`

Routing convention:
- `IJsonApiRoutingConvention`

Middleware:
- `JsonApiMiddleware`

All of these components (except for `JsonApiMiddleware`) can be customized by registering your own implementation of these services. For example:

```c#
services.AddSingleton<IJsonApiExceptionFilter, MyCustomExceptionFilter>();
```

It is also possible to directly access the .NET Core `MvcOptions` object and have full controll over which components are registered. 

## Configuring MvcOptions

JsonApiDotNetCore internally configures `MvcOptions` when calling `AddJsonApi( ... )`. However, it is still possible to register a custom configuration callback. To achieve this it is recommended to register this callback *after* the `AddJsonApi( ... )` call. It is also possible to do it earlier, but your configuration might be overridden by the JsonApiDotNetCore configuration. 

The following example replaces all internally registered filters by retrieving a custom filter from the DI container.
```c#
public class Startup
{
    private Action<MvcOptions> _postConfigureMvcOptions;

    public void ConfigureServices(IServiceCollection services)
    {
        ...

        var builder = services.AddMvcCore();
        services.AddJsonApi<AppDbContext>( ... , mvcBuilder: builder);
        mvcCoreBuilder.AddMvcOptions(x =>
        {
            // execute the mvc configuration callback after the JsonApiDotNetCore callback as been executed.
            _postConfigureMvcOptions?.Invoke(x);
        });

        ...
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
    {

        ... 

        // Using a callback, we can defer to later (when service collection has become available).
        _postConfigureMvcOptions = mvcOptions =>
        {
            mvcOptions.Filters.Clear();
            mvcOptions.Filters.Insert(0, app.ApplicationServices.GetService<CustomFilter>());
        };
        
        ...
    }
}
```
