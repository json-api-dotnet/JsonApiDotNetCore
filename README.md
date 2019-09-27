<p align="center">
<img src ="https://raw.githubusercontent.com/json-api-dotnet/JsonApiDotnetCore/master/logo.png" />
</p>

# JSON API .Net Core

[![Build status](https://ci.appveyor.com/api/projects/status/9fvgeoxdikwkom10?svg=true)](https://ci.appveyor.com/project/jaredcnance/jsonapidotnetcore)
[![Travis](https://travis-ci.org/json-api-dotnet/JsonApiDotNetCore.svg?branch=master)](https://travis-ci.org/json-api-dotnet/JsonApiDotNetCore)
[![NuGet](https://img.shields.io/nuget/v/JsonApiDotNetCore.svg)](https://www.nuget.org/packages/JsonApiDotNetCore/)
[![Join the chat at https://gitter.im/json-api-dotnet-core/Lobby](https://badges.gitter.im/json-api-dotnet-core/Lobby.svg)](https://gitter.im/json-api-dotnet-core/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![FIRST-TIMERS](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](http://www.firsttimersonly.com/)

A framework for building [json:api](http://jsonapi.org/) compliant web APIs. The ultimate goal of this library is to eliminate as much boilerplate as possible by offering out-of-the-box features such as sorting, filtering and pagination. You just need to focus on defining the resources and implementing your custom business logic. This library has been designed around dependency injection making extensibility incredibly easy.

## Getting Started

These are some steps you can take to help you understand what this project is and how you can use it:

- [What is json:api and why should I use it?](https://nordicapis.com/the-benefits-of-using-json-api/)
- [The json:api specification](http://jsonapi.org/format/)
- [Demo [Video]](https://youtu.be/KAMuo6K7VcE)
- [Our documentation](https://json-api-dotnet.github.io/JsonApiDotNetCore/)
- [Check out the example projects](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/src/Examples)
- [Embercasts: Full Stack Ember with ASP .NET Core](https://www.embercasts.com/course/full-stack-ember-with-dotnet/watch/whats-in-this-course-cs)

## Related Projects

- [Performance Reports](https://github.com/json-api-dotnet/PerformanceReports)
- [JsonApiDotNetCore.MongoDb](https://github.com/json-api-dotnet/JsonApiDotNetCore.MongoDb)
- [JsonApiDotNetCore.Marten](https://github.com/wayne-o/JsonApiDotNetCore.Marten)
- [Todo List App](https://github.com/json-api-dotnet/TodoListExample)

## Examples

See the [examples](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/src/Examples) directory for up-to-date sample applications. There is also a [Todo List App](https://github.com/json-api-dotnet/TodoListExample) that includes a JADNC API and an EmberJs client.

## Installation And Usage

See [the documentation](https://json-api-dotnet.github.io/#/) for detailed usage. 

### Models

```csharp
public class Article : Identifiable
{
    [Attr("name")]
    public string Name { get; set; }
}
```

### Controllers

```csharp
public class ArticlesController : JsonApiController<Article>
{
    public ArticlesController(
        IJsonApiContext jsonApiContext,
        IResourceService<Article> resourceService) 
    : base(jsonApiContext, resourceService) { }
}
```

### Middleware

```csharp
public class Startup 
{
    public IServiceProvider ConfigureServices(IServiceCollection services) {
        services.AddJsonApi<AppDbContext>();
        // ...
    }

    public void Configure(IApplicationBuilder app)  {
        app.UseJsonApi()
        // ...
    }
}
```

### Development

Restore all nuget packages with:

```bash
dotnet restore
```

#### Testing

Running tests locally requires access to a postgresql database.  
If you have docker installed, this can be propped up via: 

```bash
docker run --rm --name jsonapi-dotnet-core-testing  -e POSTGRES_DB=JsonApiDotNetCoreExample -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432   postgres
```

And then to run the tests:

```bash
dotnet test
```

#### Cleaning

Sometimes the compiled files can be dirty / corrupt from other branches / failed builds.

```bash
dotnet clean
```
