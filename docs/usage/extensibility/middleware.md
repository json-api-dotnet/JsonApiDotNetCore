# Middleware

It is possible to execute your own middleware before or after `JsonApiMiddleware` by registering it accordingly.

```c#
/// In Startup.Configure
app.UseMiddleware<CustomPreMiddleware>();
app.UseJsonApi();
app.UseMiddleware<CustomPostMiddleware>();
```

It is also possible to replace any other JsonApiDotNetCore middleware component. The following example replaces the internal exception filter with a custom implementation
```c#
/// In Startup.ConfigureServices
services.AddService<IJsonApiGlobalExceptionFilter, CustomExceptionFilter>()
```

Alternatively, you can add additional middleware components by configuring `MvcOptions` directly.

## Configuring MvcOptions

JsonApiDotNetCore configures `MvcOptions` internally when calling `AddJsonApi()`. Additionaly, it is possible to perform a custom configuration of `MvcOptions`. To prevent the library from overwriting your configuration, it is recommended to configure it *after* the library is done configuring `MvcOptions`.

The following example demonstrates this by clearing all internal filters and registering a custom one.
```c#
/// In Startup.ConfigureServices
services.AddSingleton<CustomFilter>();
var builder = services.AddMvcCore();
services.AddJsonApi<AppDbContext>(mvcBuilder: builder);
// Ensure the configuration action is registered after the `AddJsonApiCall`.
builder.AddMvcOptions( mvcOptions =>
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
