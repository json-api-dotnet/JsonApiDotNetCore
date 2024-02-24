using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TestBuildingBlocks;
using Xunit.Abstractions;

// ReSharper disable UnusedParameterInPartialMethod

namespace OpenApiNSwagEndToEndTests.QueryStrings.GeneratedCode;

internal partial class QueryStringsClient : JsonApiClient
{
    private readonly ILogger<QueryStringsClient>? _logger;

    public QueryStringsClient(HttpClient httpClient, ITestOutputHelper testOutputHelper)
        : this(httpClient, CreateLogger(testOutputHelper))
    {
    }

    private QueryStringsClient(HttpClient httpClient, ILogger<QueryStringsClient> logger)
        : this(httpClient)
    {
        _logger = logger;
    }

    private static ILogger<QueryStringsClient> CreateLogger(ITestOutputHelper testOutputHelper)
    {
        var loggerFactory = new LoggerFactory(new[]
        {
            new XUnitLoggerProvider(testOutputHelper, null, LogOutputFields.Message)
        });

        return loggerFactory.CreateLogger<QueryStringsClient>();
    }

    partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        SetSerializerSettingsForJsonApi(settings);

        settings.Formatting = Formatting.Indented;
    }

    partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
    {
        if (_logger != null && _logger.IsEnabled(LogLevel.Debug))
        {
            string? requestBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            _logger.LogDebug(requestBody != null ? $"--> {request}{Environment.NewLine}{Environment.NewLine}{requestBody}" : $"--> {request}");
        }
    }

    partial void ProcessResponse(HttpClient client, HttpResponseMessage response)
    {
        if (_logger != null && _logger.IsEnabled(LogLevel.Debug))
        {
            string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            _logger.LogDebug(
                !string.IsNullOrEmpty(responseBody) ? $"<-- {response}{Environment.NewLine}{Environment.NewLine}{responseBody}" : $"<-- {response}");
        }
    }
}
