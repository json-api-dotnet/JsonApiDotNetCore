# Step-By-Step Guide to a Running API

The most basic use case leverages Entity Framework Core.
The shortest path to a running API looks like:

- Create a new API project
- Install
- Define models
- Define the DbContext
- Add services and middleware
- Seed the database
- Start the API

This page will walk you through the **simplest** use case. More detailed examples can be found in the detailed usage subsections.

### Create a new API project

```
mkdir MyApi
cd MyApi
dotnet new webapi
```

### Install

```
dotnet add package JsonApiDotNetCore

- or -

Install-Package JsonApiDotNetCore
```

### Define models

Define your domain models such that they implement `IIdentifiable<TId>`.
The easiest way to do this is to inherit from `Identifiable<TId>`.

```c#
#nullable enable

[Resource]
public class Person : Identifiable<int>
{
    [Attr]
    public string Name { get; set; } = null!;
}
```

### Define the DbContext

Nothing special here, just an ordinary `DbContext`.

```
public class AppDbContext : DbContext
{
    public DbSet<Person> People => Set<Person>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}
```

### Add services and middleware

Finally, register the services and middleware by adding them to your Program.cs:

```c#
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add the Entity Framework Core DbContext like you normally would.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    string connectionString = GetConnectionString();

    // Use whatever provider you want, this is just an example.
    options.UseNpgsql(connectionString);
});

// Add JsonApiDotNetCore services.
builder.Services.AddJsonApi<AppDbContext>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.

app.UseRouting();

// Add JsonApiDotNetCore middleware.
app.UseJsonApi();

app.MapControllers();

app.Run();
```

### Seed the database

One way to seed the database is from your Program.cs:

```c#
await CreateDatabaseAsync(app.Services);

app.Run();

static async Task CreateDatabaseAsync(IServiceProvider serviceProvider)
{
    await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

    if (!dbContext.People.Any())
    {
        dbContext.People.Add(new Person
        {
            Name = "John Doe"
        });

        await dbContext.SaveChangesAsync();
    }
}
```

### Start the API

```
dotnet run
curl http://localhost:5000/people
```
