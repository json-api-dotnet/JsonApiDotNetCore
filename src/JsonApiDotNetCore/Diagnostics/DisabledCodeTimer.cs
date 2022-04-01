namespace JsonApiDotNetCore.Diagnostics;

/// <summary>
/// Doesn't record anything. Intended for Release builds and to not break existing tests.
/// </summary>
internal sealed class DisabledCodeTimer : ICodeTimer
{
    public static readonly DisabledCodeTimer Instance = new();

    private DisabledCodeTimer()
    {
    }

    public IDisposable Measure(string name)
    {
        return this;
    }

    public IDisposable Measure(string name, bool excludeInRelativeCost)
    {
        return this;
    }

    public string GetResults()
    {
        return string.Empty;
    }

    public void Dispose()
    {
    }
}
