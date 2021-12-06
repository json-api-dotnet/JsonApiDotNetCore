using JsonApiDotNetCore.Configuration;
using MultiDbContextExample.Data;
using MultiDbContextExample.Models;
using MultiDbContextExample.Repositories;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSqlite<DbContextA>("Data Source=A.db;Pooling=False");
builder.Services.AddSqlite<DbContextB>("Data Source=B.db;Pooling=False");

builder.Services.AddJsonApi(options =>
{
    options.IncludeExceptionStackTraceInErrors = true;
    options.IncludeRequestBodyInErrors = true;
}, dbContextTypes: new[]
{
    typeof(DbContextA),
    typeof(DbContextB)
});

builder.Services.AddResourceRepository<DbContextARepository<ResourceA>>();
builder.Services.AddResourceRepository<DbContextBRepository<ResourceB>>();

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

    var dbContextA = scope.ServiceProvider.GetRequiredService<DbContextA>();
    await CreateSampleDataAAsync(dbContextA);

    var dbContextB = scope.ServiceProvider.GetRequiredService<DbContextB>();
    await CreateSampleDataBAsync(dbContextB);
}

static async Task CreateSampleDataAAsync(DbContextA dbContextA)
{
    await dbContextA.Database.EnsureDeletedAsync();
    await dbContextA.Database.EnsureCreatedAsync();

    dbContextA.ResourceAs.Add(new ResourceA
    {
        NameA = "SampleA"
    });

    await dbContextA.SaveChangesAsync();
}

static async Task CreateSampleDataBAsync(DbContextB dbContextB)
{
    await dbContextB.Database.EnsureDeletedAsync();
    await dbContextB.Database.EnsureCreatedAsync();

    dbContextB.ResourceBs.Add(new ResourceB
    {
        NameB = "SampleB"
    });

    await dbContextB.SaveChangesAsync();
}
