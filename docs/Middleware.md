---
currentMenu: middleware
---

# Configure Middleware and Services

Add the following to your `Startup.ConfigureServices` method. 
Replace `AppDbContext` with your DbContext. 

```csharp
services.AddJsonApi<AppDbContext>();
```

Add the middleware to the `Startup.Configure` method. 
Note that under the hood, this will call `app.UseMvc()` 
so there is no need to add that as well.

```csharp
app.UseJsonApi();
```