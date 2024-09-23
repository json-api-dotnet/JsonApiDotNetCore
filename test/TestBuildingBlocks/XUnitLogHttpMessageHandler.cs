using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace TestBuildingBlocks;

/// <summary>
/// Writes incoming and outgoing HTTP messages to the test output window.
/// </summary>
public sealed partial class XUnitLogHttpMessageHandler : DelegatingHandler
{
    private static readonly string BodySeparator = $"{Environment.NewLine}{Environment.NewLine}";
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

            if (!string.IsNullOrEmpty(requestBody))
            {
                LogRequestMessage(request.ToString(), BodySeparator, requestBody);
            }
            else
            {
                LogRequestMessage(request.ToString(), string.Empty, string.Empty);
            }
        }

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!string.IsNullOrEmpty(responseBody))
            {
                LogResponseMessage(response.ToString(), BodySeparator, responseBody);
            }
            else
            {
                LogResponseMessage(response.ToString(), string.Empty, string.Empty);
            }
        }

        return response;
    }

    private static ILogger<XUnitLogHttpMessageHandler> CreateLogger(ITestOutputHelper testOutputHelper)
    {
        var loggerProvider = new XUnitLoggerProvider(testOutputHelper, null, LogOutputFields.Message);
        var loggerFactory = new LoggerFactory([loggerProvider]);
        return loggerFactory.CreateLogger<XUnitLogHttpMessageHandler>();
    }

    [LoggerMessage(Level = LogLevel.Debug, SkipEnabledCheck = true, Message = "--> {RequestMessage}{Separator}{RequestBody}")]
    private partial void LogRequestMessage(string requestMessage, string separator, string requestBody);

    [LoggerMessage(Level = LogLevel.Debug, SkipEnabledCheck = true, Message = "<-- {ResponseMessage}{Separator}{ResponseBody}")]
    private partial void LogResponseMessage(string responseMessage, string separator, string responseBody);
}
