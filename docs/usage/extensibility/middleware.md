# Middleware
Add the following to your Startup.ConfigureServices method. Replace AppDbContext with your DbContext.

```c3
services.AddJsonApi<AppDbContext>();
```

Add the middleware to the Startup.Configure method.

```c3
app.UseRouting();
app.UseJsonApi();
app.UseEndpoints(endpoints => endpoints.MapControllers());
```
