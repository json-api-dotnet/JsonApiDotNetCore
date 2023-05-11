using JsonApiDotNetCore.Configuration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddJsonApi(options =>
{
    options.Namespace = "api";
    options.UseRelativeLinks = true;

#if DEBUG
    options.IncludeExceptionStackTraceInErrors = true;
    options.IncludeRequestBodyInErrors = true;
    options.SerializerOptions.WriteIndented = true;
#endif
}, discovery => discovery.AddCurrentAssembly());

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.

app.UseRouting();
app.UseJsonApi();
app.MapControllers();

app.Run();
