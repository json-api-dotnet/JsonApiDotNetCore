<p align="center">
<img src ="https://raw.githubusercontent.com/json-api-dotnet/JsonApiDotNetCore/master/logo.png" />
</p>

# JsonApiDotNetCore
A framework for building [JSON:API](http://jsonapi.org/) compliant REST APIs using .NET Core and Entity Framework Core. Includes support for [Atomic Operations](https://jsonapi.org/ext/atomic/).

[![Build](https://github.com/json-api-dotnet/JsonApiDotNetCore/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/json-api-dotnet/JsonApiDotNetCore/actions/workflows/build.yml?query=branch%3Amaster)
[![Coverage](https://codecov.io/gh/json-api-dotnet/JsonApiDotNetCore/branch/master/graph/badge.svg?token=pn036tWV8T)](https://codecov.io/gh/json-api-dotnet/JsonApiDotNetCore)
[![NuGet](https://img.shields.io/nuget/v/JsonApiDotNetCore.svg)](https://www.nuget.org/packages/JsonApiDotNetCore/)
[![Chat](https://badges.gitter.im/json-api-dotnet-core/Lobby.svg)](https://gitter.im/json-api-dotnet-core/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![FIRST-TIMERS](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](http://www.firsttimersonly.com/)

The ultimate goal of this library is to eliminate as much boilerplate as possible by offering out-of-the-box features such as sorting, filtering and pagination. You just need to focus on defining the resources and implementing your custom business logic. This library has been designed around dependency injection, making extensibility incredibly easy.

## Getting Started

These are some steps you can take to help you understand what this project is and how you can use it:

### About
- [What is JSON:API and why should I use it?](https://nordicapis.com/the-benefits-of-using-json-api/) (blog, 2017)
- [Pragmatic JSON:API Design](https://www.youtube.com/watch?v=3jBJOga4e2Y) (video, 2017)
- [JSON:API and JsonApiDotNetCore](https://www.youtube.com/watch?v=79Oq0HOxyeI) (video, 2021)
- [JsonApiDotNetCore Release 4.0](https://dev.to/wunki/getting-started-5dkl) (blog, 2020)
- [JSON:API, .Net Core, EmberJS](https://youtu.be/KAMuo6K7VcE) (video, 2017)
- [Embercasts: Full Stack Ember with ASP.NET Core](https://www.embercasts.com/course/full-stack-ember-with-dotnet/watch/whats-in-this-course-cs) (paid course, 2017)

### Official documentation
- [The JSON:API specification](https://jsonapi.org/format/)
- [JsonApiDotNetCore website](https://www.jsonapi.net/)
- [Roadmap](ROADMAP.md)

## Related Projects

- [Performance Reports](https://github.com/json-api-dotnet/PerformanceReports)
- [JsonApiDotNetCore.MongoDb](https://github.com/json-api-dotnet/JsonApiDotNetCore.MongoDb)
- [Ember.js Todo List App](https://github.com/json-api-dotnet/TodoListExample)

## Examples

See the [examples](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/src/Examples) directory for up-to-date sample applications. There is also a [Todo List App](https://github.com/json-api-dotnet/TodoListExample) that includes a JsonApiDotNetCore API and an EmberJs client.

## Installation and Usage

See [our documentation](https://www.jsonapi.net/) for detailed usage.

### Models

```c#
#nullable enable

[Resource]
public class Article : Identifiable<int>
{
    [Attr]
    public string Name { get; set; } = null!;
}
```

### Middleware

```c#
// Program.cs

builder.Services.AddJsonApi<AppDbContext>();

// ...

app.UseRouting();
app.UseJsonApi();
app.MapControllers();
```

## Compatibility

The following chart should help you pick the best version, based on your environment.
See also our [versioning policy](./VERSIONING_POLICY.md).

| JsonApiDotNetCore | Status      | .NET     | Entity Framework Core |
| ----------------- | ----------- | -------- | --------------------- |
| 3.x               | Stable      | Core 2.x | 2.x                   |
| 4.x               | Stable      | Core 3.1 | 3.1                   |
|                   |             | Core 3.1 | 5                     |
|                   |             | 5        | 5                     |
|                   |             | 6        | 5                     |
| 5.0.0-5.0.2       | Stable      | 6        | 6                     |
| 5.0.3+            | Stable      | 6        | 6                     |
|                   |             | 6        | 7                     |
|                   |             | 7        | 7                     |

## Contributing

Have a question, found a bug or want to submit code changes? See our [contributing guidelines](./.github/CONTRIBUTING.md).

## Trying out the latest build

After each commit to the master branch, a new pre-release NuGet package is automatically published to [GitHub Packages](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry).
To try it out, follow the steps below:

1. [Create a Personal Access Token (classic)](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens#creating-a-personal-access-token-classic) with at least `read:packages` scope.
1. Add our package source to your local user-specific `nuget.config` file by running:
   ```bash
   dotnet nuget add source https://nuget.pkg.github.com/json-api-dotnet/index.json --name github-json-api --username YOUR-GITHUB-USERNAME --password YOUR-PAT-CLASSIC
   ```
   In the command above:
   - Replace YOUR-GITHUB-USERNAME with the username you use to login your GitHub account.
   - Replace YOUR-PAT-CLASSIC with the token your created above.
   
   :warning: If the above command doesn't give you access in the next step, remove the package source by running:
   ```bash
   dotnet nuget remove source github-json-api
   ```
   and retry with the `--store-password-in-clear-text` switch added.
1. Restart your IDE, open your project, and browse the list of packages from the github-json-api feed (make sure pre-release packages are included).

## Development

To build the code from this repository locally, run:

```bash
dotnet build
```

Running tests locally requires access to a PostgreSQL database. If you have docker installed, this can be propped up via:

```bash
pwsh run-docker-postgres.ps1
```

And then to run the tests:

```bash
dotnet test
```

Alternatively, to build, run all tests, generate code coverage and NuGet packages:

```bash
pwsh Build.ps1
```

## Sponsors

<a href="https://jb.gg/OpenSourceSupport"><img align="middle" src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.svg" alt="JetBrains Logo" style="width:150px"></a> &nbsp;
<a href="https://www.araxis.com/buy/open-source"><img align="middle" src="https://www.araxis.com/theme/37/img/araxis-logo-lg.svg" alt="Araxis Logo" style="width:150px"></a>
