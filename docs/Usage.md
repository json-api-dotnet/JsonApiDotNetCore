---
currentMenu: usage
---

# Usage

The most basic use case leverages Entity Framework. 
The shortest path to a running API looks like:

- Create a new web app
- Install
- Define models
- Define the DbContext
- Define controllers
- Add Middleware and Services
- Seed the database
- Run Migrations
- Start the app

This page will walk you through the **simplest** use case. More detailed examples can be found in the detailed usage subsections.


### Create a New Web App

```
$ mkdir MyApp
$ cd MyApp
$ dotnet new webapi
```

### Install

See [Installation](installation.html)

### Models

Define your domain models such that they implement `IIdentifiable<TId>`.
The easiest way to do this is to inherit `Identifiable`:

```csharp
public class Person : Identifiable
{ 
    [Attr("name")]
    public string Name { get; set; }
}
```

### DbContext

Nothing special here, just an ordinary DbContext

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }
        
    public DbSet<Person> People { get; set; }
}
```

### Controllers

You need to create controllers that inherit from `JsonApiController<TEntity>` or `JsonApiController<TEntity, TId>`
where `TEntity` is the model that inherits from `Identifiable<TId>`.

```csharp
public class PeopleController : JsonApiController<Person>
{
    public PeopleController(
        IJsonApiContext jsonApiContext,
        IResourceService<Person> resourceService,
        ILoggerFactory loggerFactory) 
    : base(jsonApiContext, resourceService, loggerFactory)
    { }
}
```

### Middleware and Services

Finally, add the services by adding the following to your 
`Startup.ConfigureServices`:

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services) 
{
    // add the db context like you normally would
    services.AddDbContext<AppDbContext>(options =>
    { // use whatever provider you want, this is just an example
        options.UseNpgsql(GetDbConnectionString());
    }, ServiceLifetime.Transient);

    // add jsonapi dotnet core
    services.AddJsonApi<AppDbContext>();
    // ...
}
```

Add the middleware to the `Startup.Configure` method. 
Note that under the hood, this will call `app.UseMvc()` 
so there is no need to add that as well.

```csharp
public void Configure(IApplicationBuilder app)
{
    app.UseJsonApi();
}
```

### Seeding the Database

One way to seed the database is in your Configure method:

```csharp
public void Configure(
            IApplicationBuilder app,
            AppDbContext context)
{
    context.Database.EnsureCreated();
    if(context.People.Any() == false) 
    {
        context.People.Add(new Person {
            Name = "John Doe"
        });
        context.SaveChanges();
    }
    // ...
    app.UseJsonApi();
}
```

### Run Migrations

```
$ dotnet ef migrations add AddPeople
$ dotnet ef database update
```

### Start the App

```
$ dotnet run
$ curl http://localhost:5000/people
```
