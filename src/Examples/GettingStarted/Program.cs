using GettingStarted.Data;
using GettingStarted.Models;
using JsonApiDotNetCore.Configuration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSqlite<SampleDbContext>("Data Source=sample.db;Pooling=False");

builder.Services.AddJsonApi<SampleDbContext>(options =>
{
    options.Namespace = "api";
    options.UseRelativeLinks = true;
    options.IncludeTotalResourceCount = true;
    options.SerializerOptions.WriteIndented = true;
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.

app.UseRouting();
app.UseJsonApi();
app.MapControllers();

await CreateDatabaseAsync(app.Services);

app.Run();

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
