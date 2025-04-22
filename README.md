<a href="https://www.jsonapi.net"><img src="docs/home/assets/img/logo.svg" style="height: 345px; width: 345px"/></a>

# JsonApiDotNetCore

[![Build](https://github.com/json-api-dotnet/JsonApiDotNetCore/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/json-api-dotnet/JsonApiDotNetCore/actions/workflows/build.yml?query=branch%3Amaster)
[![Coverage](https://codecov.io/gh/json-api-dotnet/JsonApiDotNetCore/branch/master/graph/badge.svg?token=pn036tWV8T)](https://codecov.io/gh/json-api-dotnet/JsonApiDotNetCore)
[![NuGet](https://img.shields.io/nuget/v/JsonApiDotNetCore.svg)](https://www.nuget.org/packages/JsonApiDotNetCore/)
[![GitHub License](https://img.shields.io/github/license/json-api-dotnet/JsonApiDotNetCore)](LICENSE)
[![Chat](https://badges.gitter.im/json-api-dotnet-core/Lobby.svg)](https://gitter.im/json-api-dotnet-core/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![FIRST-TIMERS](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://www.firsttimersonly.com/)

A framework for building [JSON:API](https://jsonapi.org/) compliant REST APIs using ASP.NET Core and Entity Framework Core. Includes support for the [Atomic Operations](https://jsonapi.org/ext/atomic/) extension.

The ultimate goal of this library is to eliminate as much boilerplate as possible by offering out-of-the-box features, such as sorting, filtering, pagination, sparse fieldset selection, and side-loading related resources. You just need to focus on defining the resources and implementing your custom business logic. This library has been designed around dependency injection, making extensibility incredibly easy.

> [!NOTE]
> OpenAPI support is now [available](https://www.jsonapi.net/usage/openapi.html), currently in preview. Give it a try!

## Getting started

The following steps describe how to create a JSON:API project.

1. Install the JsonApiDotNetCore package, along with your preferred Entity Framework Core provider:
   ```bash
   dotnet add package JsonApiDotNetCore
   dotnet add package Microsoft.EntityFrameworkCore.Sqlite
   ```

1. Declare your entities, annotated with JsonApiDotNetCore attributes:
   ```c#
   #nullable enable

   [Resource]
   public class Person : Identifiable<long>
   {
       [Attr] public string? FirstName { get; set; }
       [Attr] public string LastName { get; set; } = null!;
       [HasMany] public ISet<Person> Children { get; set; } = new HashSet<Person>();
   }
   ```

1. Define your `DbContext`, seeding the database with sample data:
   ```c#
   public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
   {
       public DbSet<Person> People => Set<Person>();

       protected override void OnConfiguring(DbContextOptionsBuilder builder)
       {
           builder.UseSqlite("Data Source=SampleDb.db");
           builder.UseAsyncSeeding(async (dbContext, _, cancellationToken) =>
           {
               dbContext.Set<Person>().Add(new Person
               {
                   FirstName = "John",
                   LastName = "Doe",
                   Children =
                   {
                       new Person
                       {
                           FirstName = "Baby",
                           LastName = "Doe"
                       }
                   }
               });
               await dbContext.SaveChangesAsync(cancellationToken);
           });
       }
   }
   ```

1. Configure Entity Framework Core and JsonApiDotNetCore in `Program.cs`:
   ```c#
   var builder = WebApplication.CreateBuilder(args);
   builder.Services.AddDbContext<AppDbContext>();
   builder.Services.AddJsonApi<AppDbContext>(options =>
   {
       options.UseRelativeLinks = true;
       options.IncludeTotalResourceCount = true;
   });

   var app = builder.Build();
   app.UseRouting();
   app.UseJsonApi();
   app.MapControllers();
   await CreateDatabaseAsync(app.Services);
   app.Run();

   static async Task CreateDatabaseAsync(IServiceProvider serviceProvider)
   {
       await using var scope = serviceProvider.CreateAsyncScope();
       var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
       await dbContext.Database.EnsureDeletedAsync();
       await dbContext.Database.EnsureCreatedAsync();
   }
   ```

1. Start your API
   ```bash
   dotnet run
   ```

1. Send a GET request to retrieve data:
   ```bash
   GET http://localhost:5000/people?filter=equals(firstName,'John')&include=children HTTP/1.1
   ```

   ```json
   {
     "links": {
       "self": "/people?filter=equals(firstName,%27John%27)&include=children",
       "first": "/people?filter=equals(firstName,%27John%27)&include=children",
       "last": "/people?filter=equals(firstName,%27John%27)&include=children"
     },
     "data": [
       {
         "type": "people",
         "id": "1",
         "attributes": {
           "firstName": "John",
           "lastName": "Doe"
         },
         "relationships": {
           "children": {
             "links": {
               "self": "/people/1/relationships/children",
               "related": "/people/1/children"
             },
             "data": [
               {
                 "type": "people",
                 "id": "2"
               }
             ]
           }
         },
         "links": {
           "self": "/people/1"
         }
       }
     ],
     "included": [
       {
         "type": "people",
         "id": "2",
         "attributes": {
           "firstName": "Baby",
           "lastName": "Doe"
         },
         "relationships": {
           "children": {
             "links": {
               "self": "/people/2/relationships/children",
               "related": "/people/2/children"
             }
           }
         },
         "links": {
           "self": "/people/2"
         }
       }
     ],
     "meta": {
       "total": 1
     }
   }
   ```

## Learn more

The following links explain what this project provides, why it exists, and how you can use it.

### About

- [What is JSON:API and why should I use it?](https://nordicapis.com/the-benefits-of-using-json-api/) (blog, 2017)
- [Pragmatic JSON:API Design](https://www.youtube.com/watch?v=3jBJOga4e2Y) (video, 2017)
- [JSON:API and JsonApiDotNetCore](https://www.youtube.com/watch?v=79Oq0HOxyeI) (video, 2021)
- [JsonApiDotNetCore Release 4.0](https://dev.to/wunki/getting-started-5dkl) (blog, 2020)
- [JSON:API, ASP.NET Core, EmberJS](https://youtu.be/KAMuo6K7VcE) (video, 2017)
- [Embercasts: Full Stack Ember with ASP.NET Core](https://www.embercasts.com/course/full-stack-ember-with-dotnet/watch/whats-in-this-course-cs) (paid course, 2017)

### Official documentation

- [JsonApiDotNetCore documentation](https://www.jsonapi.net/)
- [The JSON:API specification](https://jsonapi.org/format/)
- [JsonApiDotNetCore roadmap](ROADMAP.md)

### Samples

- The [examples](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/src/Examples) directory provides ready-to-run sample API projects
- Many advanced use cases are covered by integration tests, which can be found [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests).
  This includes topics such as batching, multi-tenancy, authorization, soft-deletion, obfuscated IDs, resource inheritance, alternate routing, custom metadata, error handling and logging.
- The [Ember.js Todo List App](https://github.com/json-api-dotnet/TodoListExample) showcases a JsonApiDotNetCore API and an Ember.js client with token authentication.

### Related projects

- [JsonApiDotNetCore.MongoDb](https://github.com/json-api-dotnet/JsonApiDotNetCore.MongoDb)
- [Ember.js Todo List App](https://github.com/json-api-dotnet/TodoListExample)
- [Performance Reports](https://github.com/json-api-dotnet/PerformanceReports)

## Compatibility

The following chart should help you pick the best version, based on your environment.
See also our [versioning policy](./VERSIONING_POLICY.md).

| JsonApiDotNetCore | Status       | .NET     | Entity Framework Core |
| ----------------- | ------------ | -------- | --------------------- |
| 3.x               | Stable       | Core 2.x | 2.x                   |
| 4.x               | Stable       | Core 3.1 | 3.1, 5                |
|                   |              | 5        | 5                     |
|                   |              | 6        | 5                     |
| 5.0.0-5.0.2       | Stable       | 6        | 6                     |
| 5.0.3-5.4.0       | Stable       | 6        | 6, 7                  |
|                   |              | 7        | 7                     |
| 5.5+              | Stable       | 6        | 6, 7                  |
|                   |              | 7        | 7                     |
|                   |              | 8        | 8, 9                  |
|                   |              | 9        | 9                     |
| master            | Preview      | 8        | 8, 9                  |
|                   |              | 9        | 9                     |

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

## Contributing

Have a question, found a bug or want to submit code changes? See our [contributing guidelines](./.github/CONTRIBUTING.md).

## Build from source

To build the code from this repository locally, run:

```bash
dotnet build
```

Running tests locally requires access to a PostgreSQL database. If you have docker installed, this can started via:

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

We are very grateful to the sponsors below, who have provided us with a no-cost license for their tools.

<a href="https://jb.gg/OpenSourceSupport"><img align="middle" src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.svg" alt="JetBrains Logo" style="width:150px"></a> &nbsp;
<a href="https://www.araxis.com/buy/open-source"><img align="middle" src="https://www.araxis.com/theme/37/img/araxis-logo-lg.svg" alt="Araxis Logo" style="width:150px"></a>

Do you like this project? Consider to [sponsor](https://github.com/sponsors/json-api-dotnet), or just reward us by giving our repository a star.
