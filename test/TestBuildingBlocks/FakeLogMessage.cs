using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace TestBuildingBlocks;

[PublicAPI]
public sealed class FakeLogMessage
{
    public LogLevel LogLevel { get; }
    public string Text { get; }

    public FakeLogMessage(LogLevel logLevel, string text)
    {
        LogLevel = logLevel;
        Text = text;
    }

    public override string ToString()
    {
        return $"[{LogLevel.ToString().ToUpperInvariant()}] {Text}";
    }
}
