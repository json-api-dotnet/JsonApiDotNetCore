# JSON API .Net Core

[![Build status](https://ci.appveyor.com/api/projects/status/9fvgeoxdikwkom10?svg=true)](https://ci.appveyor.com/project/jaredcnance/json-api-dotnet-core)
[![Travis](https://img.shields.io/travis/Research-Institute/json-api-dotnet-core.svg?maxAge=3600&label=travis)](https://travis-ci.org/Research-Institute/json-api-dotnet-core)
[![NuGet](https://img.shields.io/nuget/v/JsonApiDotNetCore.svg)](https://www.nuget.org/packages/JsonApiDotNetCore/)
[![MyGet CI](https://img.shields.io/myget/research-institute/vpre/JsonApiDotNetCore.svg)](https://www.myget.org/feed/research-institute/package/nuget/JsonApiDotNetCore)
[![Join the chat at https://gitter.im/json-api-dotnet-core/Lobby](https://badges.gitter.im/json-api-dotnet-core/Lobby.svg)](https://gitter.im/json-api-dotnet-core/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

A framework for building [json:api](http://jsonapi.org/) compliant web APIs. The ultimate goal of this library is to eliminate as much boilerplate as possible by offering out-of-the-box features such as sorting, filtering and pagination. You just need to focus on defining the resources and implementing your custom business logic. This library has been designed around dependency injection making extensibility incredibly easy.

## Installation And Usage

See the documentation [here](https://research-institute.github.io/json-api-dotnet-core)


## .Net Core v2 Notes

Branch `feat/core-2` is where I am working on .Net Core 2 compatibility tests and package upgrades.
There are several blockers to be aware of:

- Microsoft.AspNetCore.* packages target the runtime (netcoreapp) instead of netstandard. [This will be changed in future versions.](https://blogs.msdn.microsoft.com/webdev/2017/05/10/aspnet-2-preview-1/).
- Can't run acceptance testing against postgres on preview runtime [pgsql.EntityFrameworkCore.PostgreSQL#171](https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/issues/171#issuecomment-301287257)
