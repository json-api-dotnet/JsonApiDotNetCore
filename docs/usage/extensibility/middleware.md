# Middleware
Add the following to your Startup.ConfigureServices method. Replace AppDbContext with your DbContext.

```c3
services.AddJsonApi<AppDbContext>();
```

Add the middleware to the Startup.Configure method. Note that under the hood, this will call app.UseMvc() so there is no need to add that as well.

```c3
app.UseJsonApi();
```