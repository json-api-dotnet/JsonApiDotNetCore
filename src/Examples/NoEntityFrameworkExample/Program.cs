using JsonApiDotNetCore.Configuration;
using NoEntityFrameworkExample.Data;
using NoEntityFrameworkExample.Models;
using NoEntityFrameworkExample.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

string connectionString = GetConnectionString(builder.Configuration);
builder.Services.AddNpgsql<AppDbContext>(connectionString);

builder.Services.AddJsonApi(options => options.Namespace = "api/v1", resources: resourceGraphBuilder => resourceGraphBuilder.Add<WorkItem, int>());

builder.Services.AddResourceService<WorkItemService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.

app.UseRouting();
app.UseJsonApi();
app.MapControllers();

await CreateDatabaseAsync(app.Services);

app.Run();

static string GetConnectionString(IConfiguration configuration)
{
    string postgresPassword = Environment.GetEnvironmentVariable("PGPASSWORD") ?? "postgres";
    return configuration["Data:DefaultConnection"].Replace("###", postgresPassword);
}

static async Task CreateDatabaseAsync(IServiceProvider serviceProvider)
{
    await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}
