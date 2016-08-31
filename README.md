# JSON API .Net Core

[![Build status](https://ci.appveyor.com/api/projects/status/9fvgeoxdikwkom10?svg=true)](https://ci.appveyor.com/project/jaredcnance/json-api-dotnet-core)
[![Travis](https://img.shields.io/travis/Research-Institute/json-api-dotnet-core.svg?maxAge=3600&label=travis)](https://travis-ci.org/Research-Institute/json-api-dotnet-core)

JSON API Spec Conformance: **Non Conforming**

## Installation

For pre-releases, add the [MyGet](https://www.myget.org/feed/Details/research-institute) package feed 
(https://www.myget.org/F/research-institute/api/v3/index.json) 
to your nuget configuration.

NuGet packages will be published at v0.1.0. 

## Usage

Go [here](https://github.com/Research-Institute/json-api-dotnet-core/wiki/Request-Examples) to see examples of HTTP requests and responses

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

 - When you define a model, you **MUST** specify the associated resource class using the `JsonApiResource` attribute.
 - The specified resource class **MUST** implement `IJsonApiResource`. 

The resource class defines how the model will be exposed to client applications.

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

We use [AutoMapper](http://automapper.org/) to map from the model class to the resource class. 
The below snippet shows how you can specify a custom mapping expression in your `Startup` class that will append `_1` to the resource name.
Check out [AutoMapper's Wiki](https://github.com/AutoMapper/AutoMapper/wiki) for detailed mapping options.

```
services.AddJsonApi(config => {
  ...
  config.AddResourceMapping<Person, PersonResource>(map =>
  {
    // resource.Name = model.Name + "_1"
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
  config.UseController<TodoItem, TodoItemsController>();
  ...
});
```

The controller **MUST** implement `IJsonApiController`, and it **MAY** inherit from [JsonApiController](https://github.com/Research-Institute/json-api-dotnet-core/blob/master/JsonApiDotNetCore/Controllers/JsonApiController.cs).
Constructor dependency injection will work like normal. 
Any services added in your `Startup.ConfigureServices()` method will be injected into the constructor parameters.

```
public class TodoItemsController : JsonApiController, IJsonApiController
{
  private ApplicationDbContext _dbContext;

  public TodoItemsController(IJsonApiContext jsonApiContext, ResourceRepository resourceRepository, ApplicationDbContext applicationDbContext) 
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

You can access the HttpContext from [IJsonApiContext](https://github.com/Research-Institute/json-api-dotnet-core/blob/master/JsonApiDotNetCore/Abstractions/IJsonApiContext.cs).


## References
[JsonApi Specification](http://jsonapi.org/)

## Current Entity Requirements

- Using Entity Framework
- All entities in the specified context should have controllers
- All entities are served from the same namespace (i.e. 'api/v1')
- All entities have a primary key "Id" and not "EntityId"
- All entity names are proper case, "Id" not "id"
