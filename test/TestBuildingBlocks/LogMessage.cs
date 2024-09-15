using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace TestBuildingBlocks;

[PublicAPI]
public sealed class LogMessage(LogLevel logLevel, string categoryName, string text)
{
    public LogLevel LogLevel { get; } = logLevel;
    public string CategoryName { get; } = categoryName;
    public string Text { get; } = text;

    public override string ToString()
    {
        return $"[{LogLevel.ToString().ToUpperInvariant()}] {Text}";
    }
}
