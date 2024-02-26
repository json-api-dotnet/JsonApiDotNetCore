using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace TestBuildingBlocks;

[PublicAPI]
public sealed class FakeLogMessage(LogLevel logLevel, string text)
{
    public LogLevel LogLevel { get; } = logLevel;
    public string Text { get; } = text;

    public override string ToString()
    {
        return $"[{LogLevel.ToString().ToUpperInvariant()}] {Text}";
    }
}
