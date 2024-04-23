using System.Diagnostics;
using GettingStarted;
using GettingStarted.Data;
using GettingStarted.Models;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Serialization.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<SqliteSampleDbContext>(options =>
{
    options.UseSqlite("Data Source=SampleDb.db;Pooling=False");
    SetDbContextDebugOptions(options);
});

builder.Services.AddDbContext<PostgreSqlSampleDbContext>(options =>
{
    options.UseNpgsql("Host=localhost;Database=ExampleDb;User ID=postgres;Password=postgres;Include Error Detail=true");
    SetDbContextDebugOptions(options);
});

// EntityFrameworkCoreRepository injects IDbContextResolver to obtain the DbContext during a request.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IDbContextResolver, QueryStringDbContextResolver>();

// Make rendered links contain the dbType query string parameter.
builder.Services.AddScoped<ILinkBuilder, DbAwareLinkBuilder>();

// DbContext is used to scan the model at app startup. Pick any, since their entities are identical.
builder.Services.AddJsonApi<SqliteSampleDbContext>(options =>
{
    options.Namespace = "api";
    options.UseRelativeLinks = true;
    options.IncludeTotalResourceCount = true;
    options.AllowUnknownQueryStringParameters = true;

#if DEBUG
    options.IncludeExceptionStackTraceInErrors = true;
    options.IncludeRequestBodyInErrors = true;
    options.SerializerOptions.WriteIndented = true;
#endif
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.

app.UseRouting();
app.UseJsonApi();
app.MapControllers();

await CreateSqliteDatabaseAsync(app.Services);
await CreatePostgreSqlDatabaseAsync(app.Services);

app.Run();

[Conditional("DEBUG")]
static void SetDbContextDebugOptions(DbContextOptionsBuilder options)
{
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();
    options.ConfigureWarnings(builder => builder.Ignore(CoreEventId.SensitiveDataLoggingEnabledWarning));
}

static async Task CreateSqliteDatabaseAsync(IServiceProvider serviceProvider)
{
    await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<SqliteSampleDbContext>();
    await dbContext.Database.EnsureDeletedAsync();
    await dbContext.Database.EnsureCreatedAsync();

    await CreateSqliteSampleDataAsync(dbContext);
}

static async Task CreateSqliteSampleDataAsync(SqliteSampleDbContext dbContext)
{
    dbContext.Books.AddRange(new Book
    {
        Title = "Frankenstein",
        PublishYear = 1818,
        Author = new Person
        {
            Name = "Mary Shelley"
        }
    }, new Book
    {
        Title = "Robinson Crusoe",
        PublishYear = 1719,
        Author = new Person
        {
            Name = "Daniel Defoe"
        }
    }, new Book
    {
        Title = "Gulliver's Travels",
        PublishYear = 1726,
        Author = new Person
        {
            Name = "Jonathan Swift"
        }
    });

    await dbContext.SaveChangesAsync();
}

static async Task CreatePostgreSqlDatabaseAsync(IServiceProvider serviceProvider)
{
    await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<PostgreSqlSampleDbContext>();
    await dbContext.Database.EnsureDeletedAsync();
    await dbContext.Database.EnsureCreatedAsync();

    await CreatePostgreSqlSampleDataAsync(dbContext);
}

static async Task CreatePostgreSqlSampleDataAsync(PostgreSqlSampleDbContext dbContext)
{
    dbContext.Books.AddRange(new Book
    {
        Title = "Wolf Hall",
        PublishYear = 2009,
        Author = new Person
        {
            Name = "Hilary Mantel"
        }
    }, new Book
    {
        Title = "Gilead",
        PublishYear = 2004,
        Author = new Person
        {
            Name = "Marilynne Robinson"
        }
    });

    await dbContext.SaveChangesAsync();
}
