using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace TestBuildingBlocks;

/// <summary>
/// Writes incoming and outgoing HTTP messages to the test output window.
/// </summary>
public sealed class XUnitLogHttpMessageHandler : DelegatingHandler
{
    private readonly ILogger<XUnitLogHttpMessageHandler> _logger;

    public XUnitLogHttpMessageHandler(ITestOutputHelper testOutputHelper)
    {
        ArgumentNullException.ThrowIfNull(testOutputHelper);

        _logger = CreateLogger(testOutputHelper);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            string? requestBody = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

            string requestMessage = string.IsNullOrEmpty(requestBody)
                ? $"--> {request}"
                : $"--> {request}{Environment.NewLine}{Environment.NewLine}{requestBody}";

            _logger.LogDebug(requestMessage);
        }

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            string responseMessage = string.IsNullOrEmpty(responseBody)
                ? $"<-- {response}"
                : $"<-- {response}{Environment.NewLine}{Environment.NewLine}{responseBody}";

            _logger.LogDebug(responseMessage);
        }

        return response;
    }

    private static ILogger<XUnitLogHttpMessageHandler> CreateLogger(ITestOutputHelper testOutputHelper)
    {
        var loggerProvider = new XUnitLoggerProvider(testOutputHelper, null, LogOutputFields.Message);
        var loggerFactory = new LoggerFactory([loggerProvider]);
        return loggerFactory.CreateLogger<XUnitLogHttpMessageHandler>();
    }
}
