using System.Diagnostics;
using GettingStarted.Data;
using GettingStarted.Models;
using JsonApiDotNetCore.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<SampleDbContext>(options =>
{
    options.UseSqlite("Data Source=SampleDb.db;Pooling=False");
    SetDbContextDebugOptions(options);
});

builder.Services.AddJsonApi<SampleDbContext>(options =>
{
    options.Namespace = "api";
    options.UseRelativeLinks = true;
    options.IncludeTotalResourceCount = true;

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

await CreateDatabaseAsync(app.Services);

await app.RunAsync();

[Conditional("DEBUG")]
static void SetDbContextDebugOptions(DbContextOptionsBuilder options)
{
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();
    options.ConfigureWarnings(builder => builder.Ignore(CoreEventId.SensitiveDataLoggingEnabledWarning));
}

static async Task CreateDatabaseAsync(IServiceProvider serviceProvider)
{
    await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
    await dbContext.Database.EnsureDeletedAsync();
    await dbContext.Database.EnsureCreatedAsync();

    await CreateSampleDataAsync(dbContext);
}

static async Task CreateSampleDataAsync(SampleDbContext dbContext)
{
    // Note: The generate-examples.ps1 script (to create example requests in documentation) depends on these.

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
