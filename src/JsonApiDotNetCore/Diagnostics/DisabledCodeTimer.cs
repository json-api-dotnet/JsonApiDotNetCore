using System;

namespace JsonApiDotNetCore.Diagnostics
{
    /// <summary>
    /// Doesn't record anything. Intended for Release builds and to not break existing tests.
    /// </summary>
    internal sealed class DisabledCodeTimer : ICodeTimer
    {
        public static readonly DisabledCodeTimer Instance = new();

        private DisabledCodeTimer()
        {
        }

        public IDisposable Measure(string name, bool excludeInRelativeCost = false)
        {
            return this;
        }

        public string GetResult()
        {
            return string.Empty;
        }

        public void Dispose()
        {
        }
    }
}
