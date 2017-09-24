<p align="center">
<img src ="https://raw.githubusercontent.com/json-api-dotnet/JsonApiDotnetCore/master/logo.png" />
</p>

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

## Development Priorities

The current priorities for future development (in order): 
1. Operations Support (#150)
2. ASP.Net Core 2.0 Support (#161)
3. Minor features (#105, #144, #162)
4. Resource to Entity Mapping (#112)

If you're interested in working on any of the above features, take a look at the [Contributing Guide](https://github.com/json-api-dotnet/JsonApiDotNetCore/blob/master/CONTRIBUTING.MD)
or hop on the project Gitter for more direct communication.