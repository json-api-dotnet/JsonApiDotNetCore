---
currentMenu: repositories
---

# Entity Repositories

If you want to use EF, but need additional data access logic (such as authorization), you can implement custom methods for accessing the data by creating an implementation of 
[IEntityRepository&lt;Entity, TId&gt;](https://github.com/Research-Institute/json-api-dotnet-core/blob/master/src/JsonApiDotNetCore/Data/IEntityRepository.cs). If you only need minor changes you can override the 
methods defined in [DefaultEntityRepository&lt;TEntity, TId&gt;](https://github.com/Research-Institute/json-api-dotnet-core/blob/master/src/JsonApiDotNetCore/Data/DefaultEntityRepository.cs). 

The repository should then be
add to the service collection in `Startup.cs` like so:

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services) {
    services.AddScoped<IEntityRepository<MyEntity,Guid>, MyAuthorizedEntityRepository>();
    // ...
}
```

A sample implementation that performs data authorization might look like:

```csharp
public class MyAuthorizedEntityRepository : DefaultEntityRepository<MyEntity>
{
    private readonly ILogger _logger;
    private readonly IAuthenticationService _authenticationService;

    public MyAuthorizedEntityRepository(
        ILoggerFactory loggerFactory,
        IJsonApiContext jsonApiContext,
        IAuthenticationService authenticationService)
    : base(loggerFactory, jsonApiContext)
    {
        _logger = loggerFactory.CreateLogger<MyEntityRepository>();
        _authenticationService = authenticationService;
    }

    public override IQueryable<MyEntity> Get()
    {
        return base.Get().Where(e => e.UserId == _authenticationService.UserId);
    }
}
```
