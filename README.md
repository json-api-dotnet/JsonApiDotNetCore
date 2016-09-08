# JSON API .Net Core

[![Build status](https://ci.appveyor.com/api/projects/status/9fvgeoxdikwkom10?svg=true)](https://ci.appveyor.com/project/jaredcnance/json-api-dotnet-core)
[![Travis](https://img.shields.io/travis/Research-Institute/json-api-dotnet-core.svg?maxAge=3600&label=travis)](https://travis-ci.org/Research-Institute/json-api-dotnet-core)
[![NuGet](https://img.shields.io/nuget/v/JsonApiDotNetCore.svg)](https://www.nuget.org/packages/JsonApiDotNetCore/)
[![MyGet CI](https://img.shields.io/myget/research-institute/v/JsonApiDotNetCore.svg)](https://www.myget.org/feed/research-institute/package/nuget/JsonApiDotNetCore)

## Installation

`Install-Package JsonApiDotNetCore`

Click [here](https://www.nuget.org/packages/JsonApiDotNetCore/) for the latest NuGet version.

For pre-releases, add the [MyGet](https://www.myget.org/feed/Details/research-institute) package feed 
(https://www.myget.org/F/research-institute/api/v3/index.json) 
to your nuget configuration.

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

## Specifying The JsonApiResources

You can think of these as the presenter / view model definitions for your models. 

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
The below snippet is a trivial example of how you can specify a custom mapping expression in your `Startup` class.
In this example, the first and last name are concatenated and used for the value of the resource's DisplayName property.
Check out [AutoMapper's Wiki](https://github.com/AutoMapper/AutoMapper/wiki) for detailed mapping options.

```
services.AddJsonApi(config => {
  ...
  config.AddResourceMapping<Person, PersonResource>(map =>
  {
    // resource.Name = model.Name + "_1"
    map.ForMember("DisplayName", opt => opt.MapFrom(src => $"{((Person)src).FirstName} {((Person)src).LastName}"));
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

- The controller **MUST** implement `IJsonApiController`
- Controllers **MAY** inherit from [JsonApiController](https://github.com/Research-Institute/json-api-dotnet-core/blob/master/JsonApiDotNetCore/Controllers/JsonApiController.cs).

Constructor dependency injection will work like normal. 
Any services in your `Startup.ConfigureServices()` method will be injected into the constructor parameters.

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
