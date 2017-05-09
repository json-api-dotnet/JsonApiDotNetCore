---
currentMenu: services
---

# Resource Services

The [IResourceService](https://github.com/Research-Institute/json-api-dotnet-core/blob/master/src/JsonApiDotNetCore/Services/IResourceService.cs) acts as a service layer between the controller and the data access
layer. This allows you to customize it however you want and not be dependent upon Entity
Framework. This is also a good place to implement custom business logic.

### Supplementing Default Behavior

A simple example would be to send notifications when an entity gets created:

```csharp
public class TodoItemService : EntityResourceService<TodoItem> {
    
    private readonly INotificationService _notificationService;

    public TodoItemService(
        IJsonApiContext jsonApiContext,
        IEntityRepository<T, TId> repository,
        ILoggerFactory loggerFactory,
        // Get the notification service via DI
        INotificationService notificationService) 
    : base(jsonApiContext, repository, loggerFactory)
    {
        _notificationService = notificationService;
    }

    public override async Task<TEntity> CreateAsync(TEntity entity)
    {
        // call the base implementation which uses Entity Framework
        var newEntity = await base.CreateAsync(entity);
        
        // custom code
        _notificationService.Notify($"Entity created: { newEntity.Id }");

        // don't forget to return the new entity
        return entity;
    }
}
```

### Not Using Entity Framework?

As previously discussed, this library uses Entity Framework by default. 
If you'd like to use another ORM that does not implement `IQueryable`, 
you can inject a custom service like so:

```csharp
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // add the service override for MyModel
    services.AddScoped<IResourceService<MyModel>, MyModelService>();
    
    // add your own DAO
    services.AddScoped<IMyModelDAL, MyModelDAL>();
    // ...
}


// MyModelService.cs
public class MyModelService : IResourceService<MyModel>
{
    private readonly IMyModelDAL _dal;
    public MyModelService(IMyModelDAL dal)
    { 
        _dal = dal;
    } 

    public Task<IEnumerable<MyModel>> GetAsync()
    {
        return await _dal.GetModelAsync();
    }
}
```