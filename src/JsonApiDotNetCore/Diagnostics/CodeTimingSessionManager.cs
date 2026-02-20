using System.Reflection;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCore.Diagnostics;

/// <summary>
/// Provides access to the "current" measurement, which removes the need to pass along a <see cref="CascadingCodeTimer" /> instance through the entire
/// call chain.
/// </summary>
public static class CodeTimingSessionManager
{
    public static readonly bool IsEnabled = GetDefaultIsEnabled();
    private static ICodeTimerSession? _session;

    public static ICodeTimer Current
    {
        get
        {
            if (!IsEnabled)
            {
                return DisabledCodeTimer.Instance;
            }

            AssertHasActiveSession();

            return _session!.CodeTimer;
        }
    }

    private static bool GetDefaultIsEnabled()
    {
#if DEBUG
        return !IsRunningInTest() && !IsRunningInBenchmark() && !IsGeneratingOpenApiDocumentAtBuildTime();
#else
        return false;
#endif
    }

    // ReSharper disable once UnusedMember.Local
    private static bool IsRunningInTest()
    {
        const string testAssemblyName = "xunit.core";

        return AppDomain.CurrentDomain.GetAssemblies().Any(assembly =>
            assembly.FullName != null && assembly.FullName.StartsWith(testAssemblyName, StringComparison.Ordinal));
    }

    // ReSharper disable once UnusedMember.Local
    private static bool IsRunningInBenchmark()
    {
        return Assembly.GetEntryAssembly()?.GetName().Name == "Benchmarks";
    }

    // ReSharper disable once UnusedMember.Local
    private static bool IsGeneratingOpenApiDocumentAtBuildTime()
    {
        return Environment.GetCommandLineArgs().Any(argument => argument.Contains("GetDocument.Insider"));
    }

    private static void AssertHasActiveSession()
    {
        if (_session == null)
        {
            throw new InvalidOperationException($"Call {nameof(Capture)} before accessing the current session.");
        }
    }

    public static void Capture(ICodeTimerSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        AssertNoActiveSession();

        if (IsEnabled)
        {
            session.Disposed += SessionOnDisposed;
            _session = session;
        }
    }

    private static void AssertNoActiveSession()
    {
        if (_session != null)
        {
            throw new InvalidOperationException("Sessions cannot be nested. Dispose the current session first.");
        }
    }

    private static void SessionOnDisposed(object? sender, EventArgs args)
    {
#pragma warning disable IDE0031 // Use null propagation
        // Justification: Workaround for bug https://github.com/dotnet/roslyn/issues/82482.
        if (_session != null)
        {
            _session.Disposed -= SessionOnDisposed;
            _session = null;
        }
#pragma warning restore IDE0031 // Use null propagation
    }
}
