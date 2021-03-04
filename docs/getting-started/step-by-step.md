# Step-By-Step Guide to a Running API

The most basic use case leverages Entity Framework Core.
The shortest path to a running API looks like:

- Create a new web app
- Install
- Define models
- Define the DbContext
- Define controllers
- Add Middleware and Services
- Seed the database
- Start the app

This page will walk you through the **simplest** use case. More detailed examples can be found in the detailed usage subsections.

### Create A New Web App

```
mkdir MyApp
cd MyApp
dotnet new webapi
```

### Install

```
dotnet add package JsonApiDotnetCore

- or -

Install-Package JsonApiDotnetCore
```

### Define Models

Define your domain models such that they implement `IIdentifiable<TId>`.
The easiest way to do this is to inherit from `Identifiable`

```c#
public class Person : Identifiable
{
    [Attr]
    public string Name { get; set; }
}
```

### Define DbContext

Nothing special here, just an ordinary `DbContext`

```
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Person> People { get; set; }
}
```

### Define Controllers

You need to create controllers that inherit from `JsonApiController<TResource>` or `JsonApiController<TResource, TId>`
where `TResource` is the model that inherits from `Identifiable<TId>`

```c#
public class PeopleController : JsonApiController<Person>
{
    public PeopleController(IJsonApiOptions options, ILoggerFactory loggerFactory,
        IResourceService<Person> resourceService)
        : base(options, loggerFactory, resourceService)
    {
    }
}
```

### Middleware and Services

Finally, add the services by adding the following to your Startup.ConfigureServices:

```c#
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{
    // Add the Entity Framework Core DbContext like you normally would
    services.AddDbContext<AppDbContext>(options =>
    {
        // Use whatever provider you want, this is just an example
        options.UseNpgsql(GetDbConnectionString());
    });

    // Add JsonApiDotNetCore
    services.AddJsonApi<AppDbContext>();
}
```

Add the middleware to the Startup.Configure method.

```c#
// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
public void Configure(IApplicationBuilder app)
{
    app.UseRouting();
    app.UseJsonApi();
    app.UseEndpoints(endpoints => endpoints.MapControllers());
}
```

### Seeding the Database

One way to seed the database is in your Configure method:

```c#
public void Configure(IApplicationBuilder app, AppDbContext context)
{
    context.Database.EnsureCreated();

    if (!context.People.Any())
    {
        context.People.Add(new Person
        {
            Name = "John Doe"
        });

        context.SaveChanges();
    }

    app.UseRouting();
    app.UseJsonApi();
    app.UseEndpoints(endpoints => endpoints.MapControllers());
}
```

### Start the App

```
dotnet run
curl http://localhost:5000/people
```
