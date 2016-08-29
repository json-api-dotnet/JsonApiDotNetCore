# JSON API .Net Core

JSON API Spec Conformance: **Non Conforming**

Go [here](https://github.com/Research-Institute/json-api-dotnet-core/wiki/Request-Examples) to see examples of HTTP requests and responses 

## Usage

- Configure the service:

```
services.AddDbContext<ApplicationDbContext>(options =>
  options.UseNpgsql(Configuration["Data:ConnectionString"]),
  ServiceLifetime.Transient);

services.AddJsonApi(config => {
  config.SetDefaultNamespace("api/v1");
  config.UseContext<ApplicationDbContext>();
});
```

- Add middleware:

```
app.UseJsonApi();
```

## Specifying The Presenter / ViewModel

When you define a model, you **must** specify the associated `JsonApiResource` class which **must** implement `IJsonApiResource`. 
This is used for mapping out only the data that should be available to client applications.

For example:

```
[JsonApiResource(typeof(PersonResource))]
public class Person
{
  public int Id { get; set; }
  public string Name { get; set; }
  public string SomethingSecret { get; set; }
  public virtual List<TodoItem> TodoItems { get; set; }
}

public class PersonResource : IJsonApiResource
{
  public string Id { get; set; }
  public string Name { get; set; }
}
``` 

We use [AutoMapper](http://automapper.org/) to map from the context model to the JsonApiResource. 
The below snippet shows how you can specify a custom mapping expression in your `Startup` class that will apped '_1' to the resource name.
Check out [AutoMapper's Wiki](https://github.com/AutoMapper/AutoMapper/wiki) for detailed mapping options.

```
services.AddJsonApi(config => {
  ...
  config.AddResourceMapping<Person, PersonResource>(map =>
  {
    map.ForMember("Name", opt => opt.MapFrom(src => $"{((Person)src).Name}_1"));
  });
  ...
});
```

## Overriding controllers

You can define your own controllers that implement the `IJsonApiController` like so:

```
services.AddJsonApi(config => {
  ...
  config.UseController(typeof(TodoItem), typeof(TodoItemsController));
  ...
});
```

The controller **must** implement `IJsonApiController`, and it **may** inherit from `JsonApiController`.
Constructor dependency injection will work like normal. 
Any services added in your `Startup.ConfigureServices()` method will be injected into the constructor parameters.

```
public class TodoItemsController : JsonApiController, IJsonApiController
{
  private ApplicationDbContext _dbContext;

  public TodoItemsController(JsonApiContext jsonApiContext, ResourceRepository resourceRepository, ApplicationDbContext applicationDbContext) 
  : base(jsonApiContext, resourceRepository)
  {
    _dbContext = applicationDbContext;
  }

  public override ObjectResult Get()
  {
    return new OkObjectResult(_dbContext.TodoItems.ToList());
  }
}
```


## References
[JsonApi Specification](http://jsonapi.org/)

## Current Assumptions

- Using Entity Framework
- All entities in the specified context should have controllers
- All entities are served from the same namespace (i.e. 'api/v1')
- All entities have a primary key "Id" and not "EntityId"
