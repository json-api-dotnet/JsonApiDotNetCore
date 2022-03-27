using System.Diagnostics;
using BackgroundWorkerService;
using BackgroundWorkerService.Data;
using BackgroundWorkerService.Interop;
using BackgroundWorkerService.Models;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Diagnostics;
using JsonApiDotNetCore.QueryStrings;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

IHost host = Host.CreateDefaultBuilder(args).ConfigureServices((hostContext, services) =>
{
    services.AddDbContext<AppDbContext>(options =>
    {
        string connectionString = GetConnectionString(hostContext.Configuration);
        options.UseNpgsql(connectionString);
    });

    AddJsonApi(services);

    services.AddHostedService<Worker>();
}).Build();

await CreateDatabaseAsync(host.Services);

await host.RunAsync();

static string GetConnectionString(IConfiguration configuration)
{
    string postgresPassword = Environment.GetEnvironmentVariable("PGPASSWORD") ?? "postgres";
    return configuration["Data:DefaultConnection"].Replace("###", postgresPassword);
}

static void AddJsonApi(IServiceCollection serviceCollection)
{
    ReplaceCodeTimer();

    // Discard any MVC-specific service registrations, we won't need them.
    var fakeMvcBuilder = new FakeMvcBuilder();

    serviceCollection.AddJsonApi<AppDbContext>(options =>
    {
        // Avoid service registration for ModelState validation, which requires ASP.NET.
        options.ValidateModelState = false;
    }, discovery => discovery.AddCurrentAssembly(), mvcBuilder: fakeMvcBuilder);

    // Override service registration that depends on ASP.NET routing.
    serviceCollection.AddSingleton<LinkGenerator>(_ => new FakeLinkGenerator());

    // This allows us to inject a query string, since ASP.NET is unavailable.
    serviceCollection.AddScoped<IRequestQueryStringAccessor, FakeRequestQueryStringAccessor>();
}

[Conditional("DEBUG")]
static void ReplaceCodeTimer()
{
    // This is only needed when building against JADNC debug sources directly, instead of the NuGet package.
    ICodeTimerSession codeTimerSession = new DefaultCodeTimerSession();
    CodeTimingSessionManager.Capture(codeTimerSession);
}

static async Task CreateDatabaseAsync(IServiceProvider serviceProvider)
{
    await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.EnsureDeletedAsync();
    await dbContext.Database.EnsureCreatedAsync();

    await CreateSampleDataAsync(dbContext);
}

static async Task CreateSampleDataAsync(AppDbContext dbContext)
{
    var john = new Person
    {
        FirstName = "John",
        LastName = "Doe"
    };

    var jane = new Person
    {
        FirstName = "Jane",
        LastName = "Doe"
    };

    dbContext.TodoItems.Add(new TodoItem
    {
        Description = "Write code",
        Priority = TodoItemPriority.Medium,
        Owner = jane
    });

    dbContext.TodoItems.Add(new TodoItem
    {
        Description = "Write tests",
        Priority = TodoItemPriority.Medium,
        Owner = john
    });

    await dbContext.SaveChangesAsync();
}
