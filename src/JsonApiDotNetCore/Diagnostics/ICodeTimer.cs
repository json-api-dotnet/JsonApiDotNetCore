namespace JsonApiDotNetCore.Diagnostics;

/// <summary>
/// Records execution times for code blocks.
/// </summary>
public interface ICodeTimer : IDisposable
{
    /// <summary>
    /// Starts recording the duration of a code block, while including this measurement in calculated percentages. Wrap this call in a <c>using</c>
    /// statement, so the recording stops when the return value goes out of scope.
    /// </summary>
    /// <param name="name">
    /// Description of what is being recorded.
    /// </param>
    IDisposable Measure(string name);

    /// <summary>
    /// Starts recording the duration of a code block. Wrap this call in a <c>using</c> statement, so the recording stops when the return value goes out of
    /// scope.
    /// </summary>
    /// <param name="name">
    /// Description of what is being recorded.
    /// </param>
    /// <param name="excludeInRelativeCost">
    /// When set, indicates to exclude this measurement in calculated percentages.
    /// </param>
    IDisposable Measure(string name, bool excludeInRelativeCost);

    /// <summary>
    /// Returns intermediate or final results.
    /// </summary>
    string GetResults();
}
