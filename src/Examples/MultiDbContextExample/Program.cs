using System.Diagnostics;
using JsonApiDotNetCore.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MultiDbContextExample.Data;
using MultiDbContextExample.Models;
using MultiDbContextExample.Repositories;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<DbContextA>(options =>
{
    options.UseSqlite("Data Source=SampleDbA.db;Pooling=False");
    SetDbContextDebugOptions(options);
});

builder.Services.AddDbContext<DbContextB>(options =>
{
    options.UseSqlite("Data Source=SampleDbB.db;Pooling=False");
    SetDbContextDebugOptions(options);
});

builder.Services.AddResourceRepository<DbContextARepository<ResourceA>>();
builder.Services.AddResourceRepository<DbContextBRepository<ResourceB>>();

builder.Services.AddJsonApi(options =>
{
    options.Namespace = "api";
    options.UseRelativeLinks = true;
    options.IncludeTotalResourceCount = true;

#if DEBUG
    options.IncludeExceptionStackTraceInErrors = true;
    options.IncludeRequestBodyInErrors = true;
    options.SerializerOptions.WriteIndented = true;
#endif
}, dbContextTypes: new[]
{
    typeof(DbContextA),
    typeof(DbContextB)
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.

app.UseRouting();
app.UseJsonApi();
app.MapControllers();

await CreateDatabaseAsync(app.Services);

app.Run();

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
