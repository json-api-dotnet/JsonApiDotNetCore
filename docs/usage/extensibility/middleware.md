# Middleware

It is possible to replace JsonApiDotNetCore middleware components by configuring the IoC container and by configuring `MvcOptions`. 

## Configuring the IoC container 

The following example replaces the internal exception filter with a custom implementation
```c#
/// In Startup.ConfigureServices
services.AddService<IAsyncJsonApiExceptionFilter, CustomExceptionFilter>()
```

## Configuring `MvcOptions`

The following example replaces all internal filters with a custom filter.
```c#
/// In Startup.ConfigureServices
services.AddSingleton<CustomAsyncQueryStringActionFilter>();

var builder = services.AddMvcCore();
services.AddJsonApi<AppDbContext>(mvcBuilder: builder);

// Ensure this call is placed AFTER the `AddJsonApi()` call, or JsonApiDotNetCore will overwrite your configurations.
builder.AddMvcOptions(mvcOptions =>
{
    _postConfigureMvcOptions?.Invoke(mvcOptions);
});

/// In Startup.Configure
app.UseJsonApi();

_postConfigureMvcOptions = mvcOptions => 
{ 
    mvcOptions.Filters.Clear();
    mvcOptions.Filters.Insert(0, app.ApplicationServices.GetService<CustomAsyncQueryStringActionFilter>());
};
```
