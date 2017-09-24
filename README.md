<div style="text-align:center">
<img src ="https://raw.githubusercontent.com/json-api-dotnet/JsonApiDotnetCore/master/logo.png" />
</div>

# JSON API .Net Core

[![Build status](https://ci.appveyor.com/api/projects/status/9fvgeoxdikwkom10?svg=true)](https://ci.appveyor.com/project/jaredcnance/json-api-dotnet-core)
[![Travis](https://travis-ci.org/json-api-dotnet/JsonApiDotNetCore.svg?branch=master)](https://travis-ci.org/json-api-dotnet/JsonApiDotNetCore)
[![NuGet](https://img.shields.io/nuget/v/JsonApiDotNetCore.svg)](https://www.nuget.org/packages/JsonApiDotNetCore/)
[![MyGet CI](https://img.shields.io/myget/research-institute/vpre/JsonApiDotNetCore.svg)](https://www.myget.org/feed/research-institute/package/nuget/JsonApiDotNetCore)
[![Join the chat at https://gitter.im/json-api-dotnet-core/Lobby](https://badges.gitter.im/json-api-dotnet-core/Lobby.svg)](https://gitter.im/json-api-dotnet-core/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![FIRST-TIMERS](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](http://www.firsttimersonly.com/)

A framework for building [json:api](http://jsonapi.org/) compliant web APIs. The ultimate goal of this library is to eliminate as much boilerplate as possible by offering out-of-the-box features such as sorting, filtering and pagination. You just need to focus on defining the resources and implementing your custom business logic. This library has been designed around dependency injection making extensibility incredibly easy.

## Installation And Usage

See [the documentation](https://json-api-dotnet.github.io/JsonApiDotNetCore/) for detailed usage. 

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

## .Net Core v2 Notes

Branch `feat/core-2` is where I am working on .Net Core 2 compatibility tests and package upgrades.
There are several blockers to be aware of:

- Microsoft.AspNetCore.* packages target the runtime (netcoreapp) instead of netstandard. [This will be changed in future versions.](https://blogs.msdn.microsoft.com/webdev/2017/05/10/aspnet-2-preview-1/).
- Can't run acceptance testing against postgres on preview runtime [pgsql.EntityFrameworkCore.PostgreSQL#171](https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/issues/171#issuecomment-301287257)
