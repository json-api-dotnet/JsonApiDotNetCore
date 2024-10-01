using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace TestBuildingBlocks;

/// <summary>
/// Writes incoming and outgoing HTTP messages to the test output window.
/// </summary>
public sealed partial class XUnitLogHttpMessageHandler : DelegatingHandler
{
    private static readonly string BodySeparator = $"{Environment.NewLine}{Environment.NewLine}";
    private readonly LoggerFactory _loggerFactory;
    private readonly ILogger<XUnitLogHttpMessageHandler> _logger;

    public XUnitLogHttpMessageHandler(ITestOutputHelper testOutputHelper)
    {
        ArgumentNullException.ThrowIfNull(testOutputHelper);

#pragma warning disable CA2000 // Dispose objects before losing scope
        // Justification: LoggerFactory.AddProvider takes ownership (passing the provider as a constructor parameter does not).
        var loggerProvider = new XUnitLoggerProvider(testOutputHelper, null, LogOutputFields.Message);
#pragma warning restore CA2000 // Dispose objects before losing scope

        _loggerFactory = new LoggerFactory();
        _loggerFactory.AddProvider(loggerProvider);

        _logger = _loggerFactory.CreateLogger<XUnitLogHttpMessageHandler>();
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

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _loggerFactory.Dispose();
    }

    [LoggerMessage(Level = LogLevel.Debug, SkipEnabledCheck = true, Message = "--> {RequestMessage}{Separator}{RequestBody}")]
    private partial void LogRequestMessage(string requestMessage, string separator, string requestBody);

    [LoggerMessage(Level = LogLevel.Debug, SkipEnabledCheck = true, Message = "<-- {ResponseMessage}{Separator}{ResponseBody}")]
    private partial void LogResponseMessage(string responseMessage, string separator, string responseBody);
}
