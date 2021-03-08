# Middleware

It is possible to replace JsonApiDotNetCore middleware components by configuring the IoC container and by configuring `MvcOptions`. 

## Configuring the IoC container 

The following example replaces the internal exception filter with a custom implementation.

```c#
/// In Startup.ConfigureServices
services.AddService<IAsyncJsonApiExceptionFilter, CustomAsyncExceptionFilter>();
```

## Configuring `MvcOptions`

The following example replaces all internal filters with a custom filter.

```c#
public class Startup
{
    private Action<MvcOptions> _postConfigureMvcOptions;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<CustomAsyncQueryStringActionFilter>();

        IMvcCoreBuilder builder = services.AddMvcCore();
        services.AddJsonApi<AppDbContext>(mvcBuilder: builder);

        // Ensure this call is placed after the AddJsonApi call.
        builder.AddMvcOptions(mvcOptions =>
        {
            _postConfigureMvcOptions.Invoke(mvcOptions);
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Ensure this call is placed before the UseEndpoints call.
        _postConfigureMvcOptions = mvcOptions =>
        {
            mvcOptions.Filters.Clear();
            mvcOptions.Filters.Insert(0,
                app.ApplicationServices.GetService<CustomAsyncQueryStringActionFilter>());
        };

        app.UseRouting();
        app.UseJsonApi();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
```
