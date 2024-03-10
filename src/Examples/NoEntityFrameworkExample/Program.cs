using JsonApiDotNetCore.Configuration;
using NoEntityFrameworkExample;
using NoEntityFrameworkExample.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<IInverseNavigationResolver, InMemoryInverseNavigationResolver>();

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
}, discovery => discovery.AddCurrentAssembly());

builder.Services.AddSingleton(serviceProvider =>
{
    var resourceGraph = serviceProvider.GetRequiredService<IResourceGraph>();
    return resourceGraph.ToEntityModel();
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.

app.UseRouting();
app.UseJsonApi();
app.MapControllers();

app.Run();
