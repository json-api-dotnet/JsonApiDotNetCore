# Common Pitfalls

This section lists various problems we've seen users run into over the years when using JsonApiDotNetCore.
See also [Frequently Asked Questions](~/getting-started/faq.md).

#### JSON:API resources are not DTOs or ViewModels
This is a common misconception.
Similar to a database model, which consists of tables and foreign keys, JSON:API defines resources that are connected via relationships.
You're opening up a can of worms when trying to model a single table to multiple JSON:API resources.

This is best clarified using an example. Let's assume we're building a public website and an admin portal, both using the same API.
The API uses the database tables "Customers" and "LoginAccounts", having a one-to-one relationship between them.

Now let's try to define the resource classes:
```c#
[Table("Customers")]
public sealed class WebCustomer : Identifiable<long>
{
    [Attr]
    public string Name { get; set; } = null!;

    [HasOne]
    public LoginAccount? Account { get; set; }
}

[Table("Customers")]
public sealed class AdminCustomer : Identifiable<long>
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public string? CreditRating { get; set; }

    [HasOne]
    public LoginAccount? Account { get; set; }
}

[Table("LoginAccounts")]
public sealed class LoginAccount : Identifiable<long>
{
    [Attr]
    public string EmailAddress { get; set; } = null!;

    [HasOne]
    public ??? Customer { get; set; }
}
```
Did you notice the missing type of the `LoginAccount.Customer` property? We must choose between `WebCustomer` or `AdminCustomer`, but neither is correct.
This is only one of the issues you'll run into. Just don't go there.

The right way to model this is by having only `Customer` instead of `WebCustomer` and `AdminCustomer`. And then:
- Hide the `CreditRating` property for web users using [this](~/usage/extensibility/resource-definitions.md#excluding-fields) approach.
- Block web users from setting the `CreditRating` property from POST/PATCH resource endpoints by either:
  - Detecting if the `CreditRating` property has changed, such as done [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/blob/master/test/JsonApiDotNetCoreTests/IntegrationTests/InputValidation/RequestBody/WorkflowDefinition.cs).
  - Injecting `ITargetedFields`, throwing an error when it contains the `CreditRating` property.

#### JSON:API resources are not DDD domain entities
In [Domain-driven design](https://martinfowler.com/bliki/DomainDrivenDesign.html), it's considered best practice to implement business rules inside entities, with changes being controlled through an aggregate root.
This paradigm [doesn't work well](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/1092#issuecomment-932749676) with JSON:API, because each resource can be changed in isolation.
So if your API needs to guard invariants such as "the sum of all orders must never exceed 500 dollars", then you're better off with an RPC-style API instead of the REST paradigm that JSON:API follows.

Adding constructors to resource classes that validate incoming parameters before assigning them to properties does not work.
Entity Framework Core [supports](https://learn.microsoft.com/en-us/ef/core/modeling/constructors#binding-to-mapped-properties) that,
but does so via internal implementation details that are inaccessible by JsonApiDotNetCore.

In JsonApiDotNetCore, resources are what DDD calls [anemic models](https://thedomaindrivendesign.io/anemic-model/).
Validation and business rules are typically implemented in [Resource Definitions](~/usage/extensibility/resource-definitions.md).

#### Model relationships instead of foreign key attributes
It may be tempting to expose numeric resource attributes such as `customerId`, `orderId`, etc. You're better off using relationships instead, because they give you
the richness of JSON:API. For example, it enables users to include related resources in a single request, apply filters over related resources and use dedicated endpoints for updating relationships.
As an API developer, you'll benefit from rich input validation and fine-grained control for setting what's permitted when users access relationships.

#### Model relationships instead of complex (JSON) attributes
Similar to the above, returning a complex object takes away all the relationship features of JSON:API. Users can't filter inside a complex object. Or update
a nested value, without risking accidentally overwriting another unrelated nested value from a concurrent request. Basically, there's no partial PATCH to prevent that.

#### Stay away from stored procedures
There are [many reasons](https://stackoverflow.com/questions/1761601/is-the-usage-of-stored-procedures-a-bad-practice/9483781#9483781) to not use stored procedures.
But with JSON:API, there's an additional concern. Due to its dynamic nature of filtering, sorting, pagination, sparse fieldsets, and including related resources,
the number of required stored procedures to support all that either explodes, or you'll end up with one extremely complex stored proceduce to handle it all.
With stored procedures, you're either going to have a lot of work to do, or you'll end up with an API that has very limited capabilities.
Neither sounds very compelling. If stored procedures is what you need, you're better off creating an RPC-style API that doesn't use JsonApiDotNetCore.

#### Do not use `[ApiController]` on JSON:API controllers
Although recommended by Microsoft for hard-written controllers, the opinionated behavior of [`[ApiController]`](https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-7.0#apicontroller-attribute) violates the JSON:API specification.
Despite JsonApiDotNetCore trying its best to deal with it, the experience won't be as good as leaving it out.

#### Don't use auto-generated controllers with shared models

When model classes are defined in a separate project, the controllers are generated in that project as well, which is probably not what you want.
For details, see [here](~/usage/extensibility/controllers.md#auto-generated-controllers).

#### Register/override injectable services
Register your JSON:API resource services, resource definitions and repositories with `services.AddResourceService/AddResourceDefinition/AddResourceRepository()` instead of `services.AddScoped()`.
When using [Auto-discovery](~/usage/resource-graph.md#auto-discovery), you don't need to register these at all.

> [!NOTE]
> In older versions of JsonApiDotNetCore, registering your own services in the IoC container *afterwards* increased the chances that your replacements would take effect.

#### Never use the Entity Framework Core In-Memory Database Provider
When using this provider, many invalid mappings go unnoticed, leading to strange errors or wrong behavior. A real SQL engine fails to create the schema when mappings are invalid.
If you're in need of a quick setup, use [SQLite](https://www.sqlite.org/). After adding its [NuGet package](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite), it's as simple as:
```c#
// Program.cs
builder.Services.AddSqlite<AppDbContext>("Data Source=temp.db");
```
Which creates `temp.db` on disk. Simply deleting the file gives you a clean slate.
This is a lot more convenient compared to using [SqlLocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb), which runs a background service that breaks if you delete its underlying storage files.

However, even SQLite does not support all queries produced by Entity Framework Core. You'll get the best (and fastest) experience with [PostgreSQL in a docker container](https://github.com/json-api-dotnet/JsonApiDotNetCore/blob/master/run-docker-postgres.ps1).

#### One-to-one relationships require custom Entity Framework Core mappings
Entity Framework Core has great conventions and sane mapping defaults. But two of them are problematic for JSON:API: identifying foreign keys and default delete behavior.
See [here](~/usage/resources/relationships.md#one-to-one-relationships-in-entity-framework-core) for how to get it right.

#### Prefer model attributes over fluent mappings
Validation attributes such as `[Required]` are detected by ASP.NET ModelState validation, Entity Framework Core, OpenAPI, and JsonApiDotNetCore.
When using a Fluent API instead, the other frameworks cannot know about it, resulting in a less streamlined experience.

#### Validation of `[Required]` value types doesn't work
This is a limitation of ASP.NET ModelState validation. For example:
```c#
[Required] public int Age { get; set; }
```
won't cause a validation error when sending `0` or omitting it entirely in the request body.
This limitation does not apply to reference types.
The workaround is to make it nullable:
```c#
[Required] public int? Age { get; set; }
```
Entity Framework Core recognizes this and generates a non-nullable column.

#### Don't change resource property values from POST/PATCH controller methods
It simply won't work. Without going into details, this has to do with JSON:API partial POST/PATCH.
Use [Resource Definition](~/usage/extensibility/resource-definitions.md) callback methods to apply such changes from code.

#### You can't mix up pipeline methods
For example, you can't call `service.UpdateAsync()` from `controller.GetAsync()`, or call `service.SetRelationshipAsync()` from `controller.PatchAsync()`.
The reason is that various ambient injectable objects are in play, used to track what's going on during the request pipeline internally.
And they won't match up with the current endpoint when switching to a different pipeline halfway during a request.

If you need such side effects, it's easiest to inject your `DbContext` in the controller, directly apply the changes on it and save.
A better way is to inject your `DbContext` in a [Resource Definition](~/usage/extensibility/resource-definitions.md) and apply the changes there.

#### Concurrency tokens (timestamp/rowversion/xmin) won't work
While we'd love to support such [tokens for optimistic concurrency](https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=data-annotations),
it turns out that the implementation is far from trivial. We've come a long way, but aren't sure how it should work when relationship endpoints and atomic operations are involved.
If you're interested, we welcome your feedback at https://github.com/json-api-dotnet/JsonApiDotNetCore/pull/1119.
