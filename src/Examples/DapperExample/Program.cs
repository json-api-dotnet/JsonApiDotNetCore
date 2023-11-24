using System.Diagnostics;
using System.Text.Json.Serialization;
using DapperExample;
using DapperExample.AtomicOperations;
using DapperExample.Data;
using DapperExample.Models;
using DapperExample.Repositories;
using DapperExample.TranslationToSql.DataModel;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.TryAddSingleton<IClock, SystemClock>();

DatabaseProvider databaseProvider = GetDatabaseProvider(builder.Configuration);
string? connectionString = builder.Configuration.GetConnectionString($"DapperExample{databaseProvider}");

switch (databaseProvider)
{
    case DatabaseProvider.PostgreSql:
    {
        builder.Services.AddNpgsql<AppDbContext>(connectionString, optionsAction: options => SetDbContextDebugOptions(options));
        break;
    }
    case DatabaseProvider.MySql:
    {
        builder.Services.AddMySql<AppDbContext>(connectionString, ServerVersion.AutoDetect(connectionString),
            optionsAction: options => SetDbContextDebugOptions(options));

        break;
    }
    case DatabaseProvider.SqlServer:
    {
        builder.Services.AddSqlServer<AppDbContext>(connectionString, optionsAction: options => SetDbContextDebugOptions(options));
        break;
    }
}

builder.Services.AddScoped(typeof(IResourceRepository<,>), typeof(DapperRepository<,>));
builder.Services.AddScoped(typeof(IResourceWriteRepository<,>), typeof(DapperRepository<,>));
builder.Services.AddScoped(typeof(IResourceReadRepository<,>), typeof(DapperRepository<,>));

builder.Services.AddJsonApi(options =>
{
    options.UseRelativeLinks = true;
    options.IncludeTotalResourceCount = true;
    options.DefaultPageSize = null;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());

#if DEBUG
    options.IncludeExceptionStackTraceInErrors = true;
    options.IncludeRequestBodyInErrors = true;
    options.SerializerOptions.WriteIndented = true;
#endif
}, discovery => discovery.AddCurrentAssembly(), resourceGraphBuilder =>
{
    resourceGraphBuilder.Add<TodoItem, long>();
    resourceGraphBuilder.Add<Person, long>();
    resourceGraphBuilder.Add<LoginAccount, long>();
    resourceGraphBuilder.Add<AccountRecovery, long>();
    resourceGraphBuilder.Add<Tag, long>();
    resourceGraphBuilder.Add<RgbColor, int?>();
});

builder.Services.AddScoped<IInverseNavigationResolver, FromEntitiesNavigationResolver>();
builder.Services.AddSingleton<FromEntitiesDataModelService>();
builder.Services.AddSingleton<IDataModelService>(serviceProvider => serviceProvider.GetRequiredService<FromEntitiesDataModelService>());
builder.Services.AddScoped<AmbientTransactionFactory>();
builder.Services.AddScoped<IOperationsTransactionFactory>(serviceProvider => serviceProvider.GetRequiredService<AmbientTransactionFactory>());
builder.Services.AddScoped<SqlCaptureStore>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.

app.UseRouting();
app.UseJsonApi();
app.MapControllers();

await CreateDatabaseAsync(app.Services);

app.Run();

static DatabaseProvider GetDatabaseProvider(IConfiguration configuration)
{
    return configuration.GetValue<DatabaseProvider>("DatabaseProvider");
}

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

    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (await dbContext.Database.EnsureCreatedAsync())
    {
        await Seeder.CreateSampleDataAsync(dbContext);
    }
}
