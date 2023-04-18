# Frequently Asked Questions

#### Where can I find documentation and examples?
While the [documentation](~/usage/resources/index.md) covers basic features and a few runnable example projects are available [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/src/Examples),
many more advanced use cases are available as integration tests [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests), so be sure to check them out!

#### Why can't I use OpenAPI?
Due to the mismatch between the JSON:API structure and the shape of ASP.NET controller methods, this does not work out of the box.
This is high on our agenda and we're steadily making progress, but it's quite complex and far from complete.
See [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/1046) for the current status, which includes instructions on trying out the latest build.

#### What's available to implement a JSON:API client?
It depends on the programming language used. There's an overwhelming list of client libraries at https://jsonapi.org/implementations/#client-libraries.

The JSON object model inside JsonApiDotNetCore is tweaked for server-side handling (be tolerant at inputs and strict at outputs).
While you technically *could* use our `JsonSerializer` converters from a .NET client application with some hacks, we don't recommend it.
You'll need to build the resource graph on the client and rely on internal implementation details that are subject to change in future versions.

In the long term, we'd like to solve this through OpenAPI, which enables the generation of a (statically typed) client library in various languages.

#### How can I debug my API project?
Due to auto-generated controllers, you may find it hard to determine where to put your breakpoints.
In Visual Studio, controllers are accessible below **Solution Explorer > Project > Dependencies > Analyzers > JsonApiDotNetCore.SourceGenerators**.

After turning on [Source Link](https://devblogs.microsoft.com/dotnet/improving-debug-time-productivity-with-source-link/#enabling-source-link) (which enables to download the JsonApiDotNetCore source code from GitHub), you can step into our source code and add breakpoints there too.

Here are some key places in the execution pipeline to set a breakpoint:
- `JsonApiRoutingConvention.Apply`: Controllers are registered here (executes once at startup)
- `JsonApiMiddleware.InvokeAsync`: Content negotiation and `IJsonApiRequest` setup
- `QueryStringReader.ReadAll`: Parses the query string parameters
- `JsonApiReader.ReadAsync`: Parses the request body
- `OperationsProcessor.ProcessAsync`: Entry point for handling atomic operations
- `JsonApiResourceService`: Called by controllers, delegating to the repository layer
- `EntityFrameworkCoreRepository.ApplyQueryLayer`: Builds the `IQueryable<>` that is offered to Entity Framework Core (which turns it into SQL)
- `JsonApiWriter.WriteAsync`: Renders the response body
- `ExceptionHandler.HandleException`: Interception point for thrown exceptions

Aside from debugging, you can get more info by:
- Including exception stack traces and incoming request bodies in error responses, as well as writing human-readable JSON:

  ```c#
  // Program.cs
  builder.Services.AddJsonApi<AppDbContext>(options =>
  {
      options.IncludeExceptionStackTraceInErrors = true;
      options.IncludeRequestBodyInErrors = true;
      options.SerializerOptions.WriteIndented = true;
  });
  ```
- Turning on verbose logging and logging of executed SQL statements, by adding the following to your `appsettings.Development.json`:

  ```json
  {
    "Logging": {
      "LogLevel": {
        "Default": "Warning",
        "Microsoft.EntityFrameworkCore.Database.Command": "Information",
        "JsonApiDotNetCore": "Verbose"
      }
    }
  }
  ```

#### What if my JSON:API resources do not exactly match the shape of my database tables?
We often find users trying to write custom code to solve that. They usually get it wrong or incomplete, and it may not perform well.
Or it simply fails because it cannot be translated to SQL.
The good news is that there's an easier solution most of the time: configure Entity Framework Core mappings to do the work.

For example, if your primary key column is named "CustomerId" instead of "Id":
```c#
builder.Entity<Customer>().Property(x => x.Id).HasColumnName("CustomerId");
```

It certainly pays off to read up on these capabilities at [Creating and Configuring a Model](https://learn.microsoft.com/en-us/ef/core/modeling/).
Another great resource is [Learn Entity Framework Core](https://www.learnentityframeworkcore.com/configuration).

#### Can I share my resource models with .NET Framework projects?
Yes, you can. Put your model classes in a separate project that only references [JsonApiDotNetCore.Annotations](https://www.nuget.org/packages/JsonApiDotNetCore.Annotations/).
This package contains just the JSON:API attributes and targets NetStandard 1.0, which makes it flexible to consume.
At startup, use [Auto-discovery](~/usage/resource-graph.md#auto-discovery) and point it to your shared project.

#### What's the best place to put my custom business/validation logic?
For basic input validation, use the attributes from [ASP.NET ModelState Validation](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?source=recommendations&view=aspnetcore-7.0#built-in-attributes) to get the best experience.
JsonApiDotNetCore is aware of them and adjusts behavior accordingly. And it produces the best possible error responses.

For non-trivial business rules that require custom code, the place to be is [Resource Definitions](~/usage/extensibility/resource-definitions.md).
They provide a callback-based model where you can respond to everything going on.
The great thing is that your callbacks are invoked for various endpoints.
For example, the filter callback on Author executes at `GET /authors?filter=`, `GET /books/1/authors?filter=` and `GET /books?include=authors?filter[authors]=`.
Likewise, the callbacks for changing relationships execute for POST/PATCH resource endpoints, as well as POST/PATCH/DELETE relationship endpoints.

#### Can API users send multiple changes in a single request?
Yes, just activate [atomic operations](~/usage/writing/bulk-batch-operations.md).
It enables sending multiple changes in a batch request, which are executed in a database transaction.
If something fails, all changes are rolled back. The error response indicates which operation failed.

#### Is there any way to add `[Authorize(Roles = "...")]` to the generated controllers?
Sure, this is possible. Simply add the attribute at the class level.
See the docs on [Augmenting controllers](~/usage/extensibility/controllers.md#augmenting-controllers).

#### How do I expose non-JSON:API endpoints?
You can add your own controllers that do not derive from `(Base)JsonApiController` or `(Base)JsonApiOperationsController`.
Whatever you do in those is completely ignored by JsonApiDotNetCore.
This is useful if you want to add a few RPC-style endpoints or provide binary file uploads/downloads.

A middle-ground approach is to add custom action methods to existing JSON:API controllers.
While you can route them as you like, they must return JSON:API resources.
And on error, a JSON:API error response is produced.
This is useful if you want to stay in the JSON:API-compliant world, but need to expose something non-standard, for example: `GET /users/me`.

#### How do I optimize for high scalability and prevent denial of service?
Fortunately, JsonApiDotNetCore [scales pretty well](https://github.com/json-api-dotnet/PerformanceReports) under high load and/or large database tables.
It never executes filtering, sorting, or pagination in-memory and tries pretty hard to produce the most efficient query possible.
There are a few things to keep in mind, though:
- Prevent users from executing slow queries by locking down [attribute capabilities](~/usage/resources/attributes.md#capabilities) and [relationship capabilities](~/usage/resources/relationships.md#capabilities).
  Ensure the right database indexes are in place for what you enable.
- Prevent users from fetching lots of data by tweaking [maximum page size/number](~/usage/options.md#pagination) and [maximum include depth](~/usage/options.md#maximum-include-depth).
- Avoid long-running transactions by tweaking `MaximumOperationsPerRequest` in options.
- Tell your users to utilize [E-Tags](~/usage/caching.md) to reduce network traffic.
- Not included in JsonApiDotNetCore: Apply general practices such as rate limiting, load balancing, authentication/authorization, blocking very large URLs/request bodies, etc.

#### Can I offload requests to a background process?
Yes, that's possible. Override controller methods to return `HTTP 202 Accepted`, with a `Location` HTTP header where users can retrieve the result.
Your controller method needs to store the request state (URL, query string, and request body) in a queue, which your background process can read from.
From within your background process job handler, reconstruct the request state, execute the appropriate `JsonApiResourceService` method and store the result.
There's a basic example available at https://github.com/json-api-dotnet/JsonApiDotNetCore/pull/1144, which processes a captured query string.

#### What if I want to use something other than Entity Framework Core?
This basically means you'll need to implement data access yourself. There are two approaches for interception: at the resource service level and at the repository level.
Either way, you can use the built-in query string and request body parsing, as well as routing, error handling, and rendering of responses.

Here are some injectable request-scoped types to be aware of:
- `IJsonApiRequest`: This contains routing information, such as whether a primary, secondary, or relationship endpoint is being accessed.
- `ITargetedFields`: Lists the attributes and relationships from an incoming POST/PATCH resource request. Any fields missing there should not be stored (partial updates).
- `IEnumerable<IQueryConstraintProvider>`: Provides access to the parsed query string parameters.
- `IEvaluatedIncludeCache`: This tells the response serializer which related resources to render.
- `ISparseFieldSetCache`: This tells the response serializer which fields to render in the `attributes` and `relationships` objects.

You may also want to inject the singletons `IJsonApiOptions` (which contains settings such as default page size) and `IResourceGraph` (the JSON:API model of resources, attributes and relationships).

So, back to the topic of where to intercept. It helps to familiarize yourself with the [execution pipeline](~/internals/queries.md).
Replacing at the service level is the simplest. But it means you'll need to read the parsed query string parameters and invoke
all resource definition callbacks yourself. And you won't get change detection (HTTP 203 Not Modified).
Take a look at [JsonApiResourceService](https://github.com/json-api-dotnet/JsonApiDotNetCore/blob/master/src/JsonApiDotNetCore/Services/JsonApiResourceService.cs) to see what you're missing out on.

You'll get a lot more out of the box if replacing at the repository level instead. You don't need to apply options or analyze query strings.
And most resource definition callbacks are handled.
That's because the built-in resource service translates all JSON:API aspects of the request into a database-agnostic data structure called `QueryLayer`.
Now the hard part for you becomes reading that data structure and producing data access calls from that.
If your data store provides a LINQ provider, you may reuse most of [QueryableBuilder](https://github.com/json-api-dotnet/JsonApiDotNetCore/blob/master/src/JsonApiDotNetCore/Queries/Internal/QueryableBuilding/QueryableBuilder.cs),
which drives the translation into [System.Linq.Expressions](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/expression-trees/).
Note however, that it also produces calls to `.Include("")`, which is an Entity Framework Core-specific extension method, so you'll likely need to prevent that from happening. There's an example [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/blob/master/src/Examples/NoEntityFrameworkExample/Repositories/InMemoryResourceRepository.cs).
We use a similar approach for accessing [MongoDB](https://github.com/json-api-dotnet/JsonApiDotNetCore.MongoDb/blob/674889e037334e3f376550178ce12d0842d7560c/src/JsonApiDotNetCore.MongoDb/Queries/Internal/QueryableBuilding/MongoQueryableBuilder.cs).

> [!TIP]
> [ExpressionTreeVisualizer](https://github.com/zspitz/ExpressionTreeVisualizer) is very helpful in trying to debug LINQ expression trees!

#### I love JsonApiDotNetCore! How can I support the team?
The best way to express your gratitude is by starring our repository.
This increases our leverage when asking for bug fixes in dependent projects, such as the .NET runtime and Entity Framework Core.
Of course, a simple thank-you message in our [Gitter channel](https://gitter.im/json-api-dotnet-core/Lobby) is appreciated too!
We don't take monetary contributions at the moment.

If you'd like to do more: try things out, ask questions, create GitHub bug reports or feature requests, or upvote existing issues that are important to you.
We welcome PRs, but keep in mind: The worst thing in the world is opening a PR that gets rejected after you've put a lot of effort into it.
So for any non-trivial changes, please open an issue first to discuss your approach and ensure it fits the product vision.

#### Is there anything else I should be aware of?
See [Common Pitfalls](~/usage/common-pitfalls.md).
