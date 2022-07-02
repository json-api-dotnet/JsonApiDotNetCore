using System.Diagnostics;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Diagnostics;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Repositories;
using Microsoft.AspNetCore.Routing;
using NoDbConsoleQueryExample;
using NoDbConsoleQueryExample.Interop;
using NoDbConsoleQueryExample.Models;
using NoDbConsoleQueryExample.Repositories;

IHost host = Host.CreateDefaultBuilder(args).ConfigureServices(services =>
{
    services.AddHostedService<Worker>();
    services.Configure<HostOptions>(options => options.ShutdownTimeout = Timeout.InfiniteTimeSpan);
    AddHostedJsonApi(services);
}).Build();

await host.StartAsync();
await host.StopAsync();
await host.WaitForShutdownAsync();

static void AddHostedJsonApi(IServiceCollection serviceCollection)
{
    ReplaceCodeTimer();

    // Discard any MVC-specific service registrations, we won't need them.
    var fakeMvcBuilder = new FakeMvcBuilder();

    serviceCollection.AddJsonApi(options =>
    {
        // Avoid service registration for ModelState validation, which requires ASP.NET.
        options.ValidateModelState = false;
    }, null, builder =>
    {
        builder.Add<Track, long>();
        builder.Add<Artist, long>();
        builder.Add<Genre, long>();
    }, fakeMvcBuilder);

    // Override service registration that depends on ASP.NET routing.
    serviceCollection.AddSingleton<LinkGenerator>(_ => new HiddenLinkGenerator());

    // This allows us to inject a query string, since ASP.NET is unavailable.
    serviceCollection.AddScoped<IRequestQueryStringAccessor, InjectableRequestQueryStringAccessor>();

    // Use generic ObjectRepository for in-memory data, instead of default EntityFrameworkCoreRepository.
    serviceCollection.AddScoped(typeof(IResourceReadRepository<,>), typeof(ObjectRepository<,>));
    serviceCollection.AddScoped(typeof(IResourceRepository<,>), typeof(ObjectRepository<,>));

    serviceCollection.AddScoped(typeof(IDataSourceProvider<,>), typeof(ObjectDataSourceProvider<,>));
}

[Conditional("DEBUG")]
static void ReplaceCodeTimer()
{
    // This is only needed when building against JADNC debug sources directly, instead of the NuGet package.
    ICodeTimerSession codeTimerSession = new DefaultCodeTimerSession();
    CodeTimingSessionManager.Capture(codeTimerSession);
}
