using DatabasePerTenantExample.Data;
using DatabasePerTenantExample.Models;
using JsonApiDotNetCore.Configuration;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql());

builder.Services.AddJsonApi<AppDbContext>(options =>
{
    options.Namespace = "api";
    options.UseRelativeLinks = true;
    options.SerializerOptions.WriteIndented = true;
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.

app.UseRouting();
app.UseJsonApi();
app.MapControllers();

await CreateDatabaseAsync(null, app.Services);
await CreateDatabaseAsync("AdventureWorks", app.Services);
await CreateDatabaseAsync("Contoso", app.Services);

app.Run();

static async Task CreateDatabaseAsync(string? tenantName, IServiceProvider serviceProvider)
{
    await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (tenantName != null)
    {
        dbContext.SetTenantName(tenantName);
    }

    await dbContext.Database.EnsureDeletedAsync();
    await dbContext.Database.EnsureCreatedAsync();

    if (tenantName != null)
    {
        dbContext.Employees.Add(new Employee
        {
            FirstName = "John",
            LastName = "Doe",
            CompanyName = tenantName
        });

        await dbContext.SaveChangesAsync();
    }
}
