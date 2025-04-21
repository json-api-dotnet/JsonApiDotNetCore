#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCoreExample;

internal static partial class AppLog
{
    [LoggerMessage(Level = LogLevel.Information, SkipEnabledCheck = true, Message = "Measurement results for application startup:{LineBreak}{TimingResults}")]
    public static partial void LogStartupTimings(ILogger logger, string lineBreak, string timingResults);
}
