using BackgroundWorkerService.Interop;
using BackgroundWorkerService.Models;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Services;

namespace BackgroundWorkerService;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const string queryString = "?filter=equals(owner.firstName,'John')";

        while (!stoppingToken.IsCancellationRequested)
        {
            await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
            await ExecuteInScopeAsync(scope.ServiceProvider, queryString, stoppingToken);

            await Task.Delay(2000, stoppingToken);
        }
    }

    private async Task ExecuteInScopeAsync(IServiceProvider serviceProvider, string queryString, CancellationToken cancellationToken)
    {
        // JsonApiMiddleware depends on ASP.NET, so we must setup IJsonApiRequest ourselves.
        SetupJsonApiRequest(serviceProvider);

        // ASP.NET action filters do not execute, so we must invoke the query string reader ourselves.
        ParseQueryString(serviceProvider, queryString);

        // Controllers depend on ASP.NET, so instead we invoke from the resource service layer.
        var todoItemService = serviceProvider.GetRequiredService<IResourceService<TodoItem, int>>();
        IReadOnlyCollection<TodoItem> todoItems = await todoItemService.GetAsync(cancellationToken);

        _logger.LogInformation($"Found {todoItems.Count} matching items.");
    }

    private static void SetupJsonApiRequest(IServiceProvider serviceProvider)
    {
        var resourceGraph = serviceProvider.GetRequiredService<IResourceGraph>();

        var request = (JsonApiRequest)serviceProvider.GetRequiredService<IJsonApiRequest>();
        request.Kind = EndpointKind.Primary;
        request.PrimaryResourceType = resourceGraph.GetResourceType<TodoItem>();
        request.IsCollection = true;
        request.IsReadOnly = true;
    }

    private static void ParseQueryString(IServiceProvider serviceProvider, string queryString)
    {
        var queryStringAccessor = (FakeRequestQueryStringAccessor)serviceProvider.GetRequiredService<IRequestQueryStringAccessor>();
        queryStringAccessor.SetFromText(queryString);

        var queryStringReader = serviceProvider.GetRequiredService<IQueryStringReader>();
        queryStringReader.ReadAll(null);
    }
}
