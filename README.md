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

# Generators

You can install the [Yeoman generators](https://github.com/Research-Institute/json-api-dotnet-core-generators) 
to make building applications much easier.

## Usage

You need to do 3 things:

- Add Middleware and Services
- Define Models
- Define Controllers

I recommend reading the details below, but once you're familiar with the
setup, you can use the Yeoman generator to generate the required classes.

## Middleware and Services

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

## Defining Models

Your models should inherit `Identifiable<TId>` where `TId` is the type of the primary key, like so:

```csharp
public class Person : Identifiable<Guid>
{
    public override Guid Id { get; set; }
}
```

### Specifying Public Attributes

If you want an attribute on your model to be publicly available, 
add the `AttrAttribute` and provide the outbound name.

```csharp
public class Person : Identifiable<int>
{
    public override int Id { get; set; }
    
    [Attr("first-name")]
    public string FirstName { get; set; }
}
```

### Relationships

In order for navigation properties to be identified in the model, 
they should be labeled as virtual.

```csharp
public class Person : Identifiable<int>
{
    public override int Id { get; set; }
    
    [Attr("first-name")]
    public string FirstName { get; set; }

    public virtual List<TodoItem> TodoItems { get; set; }
}
```

Dependent relationships should contain a property in the form `{RelationshipName}Id`. 
For example, a `TodoItem` may have an `Owner` and so the Id attribute should be `OwnerId` like so:

```csharp
public class TodoItem : Identifiable<int>
{
    public override int Id { get; set; }
    
    [Attr("description")]
    public string Description { get; set; }

    public int OwnerId { get; set; }
    public virtual Person Owner { get; set; }
}
```

## Defining Controllers

You need to create controllers that inherit from `JsonApiController<TEntity>` or `JsonApiController<TEntity, TId>`
where `TEntity` is the model that inherits from `Identifiable<TId>`.

```csharp
[Route("api/[controller]")]
public class ThingsController : JsonApiController<Thing>
{
    public ThingsController(
        IJsonApiContext jsonApiContext,
        IEntityRepository<Thing> entityRepository,
        ILoggerFactory loggerFactory) 
    : base(jsonApiContext, entityRepository, loggerFactory)
    { }
}
```

### Non-Integer Type Keys

If your model is using a type other than `int` for the primary key,
you should explicitly declare it in the controller
and repository generic type definitions:

```csharp
[Route("api/[controller]")]
public class ThingsController : JsonApiController<Thing, Guid>
{
    public ThingsController(
        IJsonApiContext jsonApiContext,
        IEntityRepository<Thing, Guid> entityRepository,
        ILoggerFactory loggerFactory) 
    : base(jsonApiContext, entityRepository, loggerFactory)
    { }
}
```

## Routing

By default the library will configure routes for each controller. 
Based on the [recommendations](http://jsonapi.org/recommendations/)
outlined in the JSONAPI spec, routes are hyphenated. For example:

```
/todo-items --> TodoItemsController
NOT /todoItems
```

### Namespacing and Versioning URLs

You can add a namespace to the URL by specifying it in `ConfigureServices`:

```csharp
services.AddJsonApi<AppDbContext>(
    opt => opt.Namespace = "api/v1");
```

## Pagination

If you would like pagination implemented by default, you can specify the page size
when setting up the services:

```csharp
 services.AddJsonApi<AppDbContext>(
     opt => opt.DefaultPageSize = 10);
```

## Defining Custom Data Access Methods

You can implement custom methods for accessing the data by creating an implementation of 
`IEntityRepository<TEntity, TId>`. If you only need minor changes you can override the 
methods defined in `DefaultEntityRepository<TEntity, TId>`. The repository should then be
add to the service collection in `Startup.ConfigureServices` like so:

```
services.AddScoped<IEntityRepository<MyEntity,Guid>, MyEntityRepository>();
```

## Filtering

You can filter resources by attributes using the `filter` query parameter. 
By default, all attributes are filterable.
The filtering strategy we have selected, uses the following form:

```
?filter[attribute]=value
```

For operations other than equality, the query can be prefixed with an operation
identifier):

```
?filter[attribute]=eq:value
?filter[attribute]=lt:value
?filter[attribute]=gt:value
?filter[attribute]=le:value
?filter[attribute]=ge:value
```

# Tests

## Running

I am using DotNetCoreDocs to generate sample requests and documentation.

1. To run the tests, start a postgres server and verify the connection properties define in `/test/JsonApiDotNetCoreExampleTests/appsettings.json`
2. `cd ./test/JsonApiDotNetCoreExampleTests`
3. `dotnet test`
4. `cd ./src/JsonApiDotNetCoreExample`
5. `dotnet run`
6. `open http://localhost:5000/docs`
