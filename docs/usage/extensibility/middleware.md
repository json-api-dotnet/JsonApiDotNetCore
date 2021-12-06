# Middleware

The default middleware validates incoming `Content-Type` and `Accept` HTTP headers.
Based on routing configuration, it fills `IJsonApiRequest`, an injectable object that contains JSON:API-related information about the request being processed.

It is possible to replace the built-in middleware components by configuring the IoC container and by configuring `MvcOptions`. 

## Configuring the IoC container 

The following example replaces the internal exception filter with a custom implementation.

```c#
// Program.cs
builder.Services.AddService<IAsyncJsonApiExceptionFilter, CustomAsyncExceptionFilter>();
```

## Configuring `MvcOptions`

The following example replaces the built-in query string action filter with a custom filter.

```c#
// Program.cs

// Add services to the container.

builder.Services.AddScoped<CustomAsyncQueryStringActionFilter>();

IMvcCoreBuilder mvcCoreBuilder = builder.Services.AddMvcCore();
builder.Services.AddJsonApi<AppDbContext>(mvcBuilder: mvcCoreBuilder);

Action<MvcOptions>? postConfigureMvcOptions = null;

// Ensure this is placed after the AddJsonApi() call.
mvcCoreBuilder.AddMvcOptions(mvcOptions =>
{
    postConfigureMvcOptions?.Invoke(mvcOptions);
});

// Configure the HTTP request pipeline.

// Ensure this is placed before the MapControllers() call.
postConfigureMvcOptions = mvcOptions =>
{
    IFilterMetadata existingFilter = mvcOptions.Filters.Single(filter =>
        filter is ServiceFilterAttribute serviceFilter &&
        serviceFilter.ServiceType == typeof(IAsyncQueryStringActionFilter));

    mvcOptions.Filters.Remove(existingFilter);

    using IServiceScope scope = app.Services.CreateScope();

    var newFilter =
        scope.ServiceProvider.GetRequiredService<CustomAsyncQueryStringActionFilter>();

    mvcOptions.Filters.Insert(0, newFilter);
};

app.UseRouting();
app.UseJsonApi();
app.MapControllers();

app.Run();
```
