# JSON API .Net Core

[![Build status](https://ci.appveyor.com/api/projects/status/9fvgeoxdikwkom10?svg=true)](https://ci.appveyor.com/project/jaredcnance/json-api-dotnet-core)
[![Travis](https://img.shields.io/travis/Research-Institute/json-api-dotnet-core.svg?maxAge=3600&label=travis)](https://travis-ci.org/Research-Institute/json-api-dotnet-core)
[![NuGet](https://img.shields.io/nuget/v/JsonApiDotNetCore.svg)](https://www.nuget.org/packages/JsonApiDotNetCore/)
[![MyGet CI](https://img.shields.io/myget/research-institute/vpre/JsonApiDotNetCore.svg)](https://www.myget.org/feed/research-institute/package/nuget/JsonApiDotNetCore)
[![Join the chat at https://gitter.im/json-api-dotnet-core/Lobby](https://badges.gitter.im/json-api-dotnet-core/Lobby.svg)](https://gitter.im/json-api-dotnet-core/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

A framework for building [json:api](http://jsonapi.org/) compliant web servers. It allows you to eliminate a significant amount of boilerplate while offering out-of-the-box features such as sorting, filtering and pagination. This library provides all the required middleware to build a complete server. All you need to focus on is defining the resources and implementing your custom business logic. This library has been designed around dependency injection making extensibility incredibly easy.

## Installation And Usage

See the documentation [here](https://research-institute.github.io/json-api-dotnet-core)


## .Net Core v2 Notes

Branch `feat/core-2` is where I am working on .Net Core 2 compatibility tests and package upgrades.
There are several blockers to be aware of:

- Microsoft.AspNetCore.* packages target the runtime (netcoreapp) instead of netstandard. 
This will be fixed in future versions.
- EF bug against netcoreapp2.0 runtime ([EntityFramework#8021](https://github.com/aspnet/EntityFramework/issues/8021))
- Can't run acceptance testing against postgres on preview runtime [pgsql.EntityFrameworkCore.PostgreSQL#171](https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/issues/171#issuecomment-301287257)
