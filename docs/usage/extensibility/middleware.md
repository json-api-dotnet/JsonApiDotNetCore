# Middleware

It is possible to replace JsonApiDotNetCore middleware components by configuring the IoC container and by configuring `MvcOptions`. 

## Configuring the IoC container 

The following example replaces the internal exception filter with a custom implementation
```c#
/// In Startup.ConfigureServices
services.AddService<IJsonApiGlobalExceptionFilter, CustomExceptionFilter>()
```

## Configuring `MvcOptions`

To prevent the library from overwriting your configuration, perform your configuration *after* the library is done configuring `MvcOptions`. The following example replaces all internal filters with a custom filter.
```c#
/// In Startup.ConfigureServices
services.AddSingleton<CustomFilter>();
var builder = services.AddMvcCore();
services.AddJsonApi<AppDbContext>(mvcBuilder: builder);

// Ensure the configuration action is registered after the `AddJsonApi()` call.
builder.AddMvcOptions(mvcOptions =>
{
    // Execute the MvcOptions configuration callback after the JsonApiDotNetCore callback as been executed.
    _postConfigureMvcOptions?.Invoke(mvcOptions);
});

/// In Startup.Configure
app.UseJsonApi();

// Ensure the configuration callback is set after calling `UseJsonApi()`.
_postConfigureMvcOptions = mvcOptions => 
{ 
    mvcOptions.Filters.Clear();
    mvcOptions.Filters.Insert(0, app.ApplicationServices.GetService<CustomFilter>());
};
```
