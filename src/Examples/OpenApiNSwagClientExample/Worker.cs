using JsonApiDotNetCore.OpenApi.Client.NSwag;

namespace OpenApiNSwagClientExample;

public sealed class Worker(ExampleApiClient apiClient, IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    private readonly ExampleApiClient _apiClient = apiClient;
    private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            ApiResponse<DataInRequest> response1 = await _apiClient.GetBuildingCollectionAsync(stoppingToken);
            ApiResponse<ResidenceDataInRequest> response2 = await _apiClient.GetResidenceCollectionAsync(stoppingToken);
        }
        /*catch (ApiException<ErrorResponseDocument> exception)
        {
            Console.WriteLine($"JSON:API ERROR: {exception.Result.Errors.First().Detail}");
        }*/
        catch (HttpRequestException exception)
        {
            Console.WriteLine($"ERROR: {exception.Message}");
        }

        _hostApplicationLifetime.StopApplication();
    }
}
