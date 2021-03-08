<p align="center">
<img src ="https://raw.githubusercontent.com/json-api-dotnet/JsonApiDotnetCore/master/logo.png" />
</p>

# JsonApiDotNetCore
A framework for building [JSON:API](http://jsonapi.org/) compliant REST APIs using .NET Core and Entity Framework Core.

[![Build status](https://ci.appveyor.com/api/projects/status/5go47hrm0iik0ls3/branch/master?svg=true)](https://ci.appveyor.com/project/jaredcnance/jsonapidotnetcore/branch/master)
[![Travis](https://travis-ci.org/json-api-dotnet/JsonApiDotNetCore.svg?branch=master)](https://travis-ci.org/json-api-dotnet/JsonApiDotNetCore)
[![NuGet](https://img.shields.io/nuget/v/JsonApiDotNetCore.svg)](https://www.nuget.org/packages/JsonApiDotNetCore/)
[![Join the chat at https://gitter.im/json-api-dotnet-core/Lobby](https://badges.gitter.im/json-api-dotnet-core/Lobby.svg)](https://gitter.im/json-api-dotnet-core/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![FIRST-TIMERS](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](http://www.firsttimersonly.com/)

The ultimate goal of this library is to eliminate as much boilerplate as possible by offering out-of-the-box features such as sorting, filtering and pagination. You just need to focus on defining the resources and implementing your custom business logic. This library has been designed around dependency injection, making extensibility incredibly easy.

## Getting Started

These are some steps you can take to help you understand what this project is and how you can use it:

- [What is JSON:API and why should I use it?](https://nordicapis.com/the-benefits-of-using-json-api/)
- [The JSON:API specification](http://jsonapi.org/format/)
- Demo [Video](https://youtu.be/KAMuo6K7VcE), [Blog](https://dev.to/wunki/getting-started-5dkl)
- [Our documentation](https://www.jsonapi.net/)
- [Check out the example projects](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/src/Examples)
- [Embercasts: Full Stack Ember with ASP.NET Core](https://www.embercasts.com/course/full-stack-ember-with-dotnet/watch/whats-in-this-course-cs)
- [Roadmap](ROADMAP.md)

## Related Projects

- [Performance Reports](https://github.com/json-api-dotnet/PerformanceReports)
- [JsonApiDotNetCore.MongoDb](https://github.com/json-api-dotnet/JsonApiDotNetCore.MongoDb)
- [JsonApiDotNetCore.Marten](https://github.com/wayne-o/JsonApiDotNetCore.Marten)
- [Todo List App](https://github.com/json-api-dotnet/TodoListExample)

## Examples

See the [examples](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/src/Examples) directory for up-to-date sample applications. There is also a [Todo List App](https://github.com/json-api-dotnet/TodoListExample) that includes a JADNC API and an EmberJs client.

## Installation and Usage

See [our documentation](https://www.jsonapi.net/) for detailed usage.

### Models

```c#
public class Article : Identifiable
{
    [Attr]
    public string Name { get; set; }
}
```

### Controllers

```c#
public class ArticlesController : JsonApiController<Article>
{
    public ArticlesController(IJsonApiOptions options, IResourceService<Article> resourceService,
        ILoggerFactory loggerFactory)
        : base(options, resourceService, loggerFactory)
    {
    }
}
```

### Middleware

```c#
public class Startup
{
    public IServiceProvider ConfigureServices(IServiceCollection services)
    {
        services.AddJsonApi<AppDbContext>();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseJsonApi();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
```

## Development

Restore all NuGet packages with:

```bash
dotnet restore
```

### Testing

Running tests locally requires access to a PostgreSQL database.  If you have docker installed, this can be propped up via:

```bash
docker run --rm --name jsonapi-dotnet-core-testing  -e POSTGRES_DB=JsonApiDotNetCoreExample -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres:12.0
```

And then to run the tests:

```bash
dotnet test
```

## Contributing

Have a question, found a bug or want to submit code changes? See our [contributing guidelines](./.github/CONTRIBUTING.md).

## Compatibility

A lot of changes were introduced in v4, the following chart should help you with compatibility issues between .NET Core versions.

| .NET Core Version | EF Core Version | JADNC Version |
| ----------------- | --------------- | ------------- |
| 2.x               | 2.x             | v3.x          |
| 3.x               | 3.x, 5.x        | v4.x          |
| 5.x               | 5.x             | v4.x          |

### Trying out the latest build

After each commit, a new prerelease NuGet package is automatically published to AppVeyor at https://ci.appveyor.com/nuget/jsonapidotnetcore. To try it out, follow the next steps:

* In Visual Studio: **Tools**, **NuGet Package Manager**, **Package Manager Settings**, **Package Sources**
    * Click **+**
    * Name: **AppVeyor JADNC**, Source: **https://ci.appveyor.com/nuget/jsonapidotnetcore**
    * Click **Update**, **Ok**
* Open the NuGet package manager console (**Tools**, **NuGet Package Manager**, **Package Manager Console**)
    * Select **AppVeyor JADNC** as package source
    * Run command: `Install-Package JonApiDotNetCore -pre`
