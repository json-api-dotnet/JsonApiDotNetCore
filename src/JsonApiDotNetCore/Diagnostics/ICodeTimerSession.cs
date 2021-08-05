using System;

namespace JsonApiDotNetCore.Diagnostics
{
    /// <summary>
    /// Removes the need to pass along a <see cref="CascadingCodeTimer" /> instance through the entire call chain when using code timing.
    /// </summary>
    internal interface ICodeTimerSession : IDisposable
    {
        ICodeTimer CodeTimer { get; }

        event EventHandler Disposed;
    }
}
