using JsonApiDotNetCore.OpenApi.Client.Kiota;
using OpenApiKiotaClientExample.GeneratedCode;
using OpenApiKiotaClientExample.GeneratedCode.Models;

namespace OpenApiKiotaClientExample;

public sealed class Worker(ExampleApiClient apiClient, IHostApplicationLifetime hostApplicationLifetime, SetQueryStringHttpMessageHandler queryStringHandler)
    : BackgroundService
{
    private readonly ExampleApiClient _apiClient = apiClient;
    private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;
    private readonly SetQueryStringHttpMessageHandler _queryStringHandler = queryStringHandler;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            DataInRequest? response1 = await _apiClient.Api.Buildings.GetAsync(cancellationToken: stoppingToken);
            ResidenceDataInRequest? response2 = await _apiClient.Api.Residences.GetAsync(cancellationToken: stoppingToken);
        }
        /*catch (ErrorResponseDocument exception)
        {
            Console.WriteLine($"JSON:API ERROR: {exception.Errors!.First().Detail}");
        }*/
        catch (HttpRequestException exception)
        {
            Console.WriteLine($"ERROR: {exception.Message}");
        }

        _hostApplicationLifetime.StopApplication();
    }
}
