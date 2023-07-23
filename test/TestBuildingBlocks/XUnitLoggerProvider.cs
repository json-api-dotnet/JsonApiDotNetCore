using System.Text;
using JsonApiDotNetCore;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace TestBuildingBlocks;

// Based on https://www.meziantou.net/how-to-get-asp-net-core-logs-in-the-output-of-xunit-tests.htm.
public sealed class XUnitLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly LogOutputFields _outputFields;

    public XUnitLoggerProvider(ITestOutputHelper testOutputHelper, LogOutputFields outputFields = LogOutputFields.All)
    {
        ArgumentGuard.NotNull(testOutputHelper);

        _testOutputHelper = testOutputHelper;
        _outputFields = outputFields;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new XUnitLogger(_testOutputHelper, _outputFields, categoryName);
    }

    public void Dispose()
    {
    }

    private sealed class XUnitLogger : ILogger
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly LogOutputFields _outputFields;
        private readonly string _categoryName;
        private readonly IExternalScopeProvider _scopeProvider = new NoExternalScopeProvider();

        public XUnitLogger(ITestOutputHelper testOutputHelper, LogOutputFields outputFields, string categoryName)
        {
            ArgumentGuard.NotNull(testOutputHelper);
            ArgumentGuard.NotNull(categoryName);

            _testOutputHelper = testOutputHelper;
            _outputFields = outputFields;
            _categoryName = categoryName;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
#if DEBUG
            return logLevel != LogLevel.None;
#else
            return false;
#endif
        }

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
        {
            return _scopeProvider.Push(state);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var builder = new StringBuilder();

            if (_outputFields.HasFlag(LogOutputFields.Level))
            {
                string logLevelString = GetLogLevelString(logLevel);
                builder.Append(logLevelString);
            }

            if (_outputFields.HasFlag(LogOutputFields.Category))
            {
                if (builder.Length > 0)
                {
                    builder.Append(' ');
                }

                builder.Append('[');
                builder.Append(_categoryName);
                builder.Append(']');
            }

            if (_outputFields.HasFlag(LogOutputFields.Message))
            {
                if (builder.Length > 0)
                {
                    builder.Append(' ');
                }

                string message = formatter(state, exception);
                builder.Append(message);
            }

            if (exception != null && _outputFields.HasFlag(LogOutputFields.Exception))
            {
                builder.Append('\n');
                builder.Append(exception);
            }

            if (_outputFields.HasFlag(LogOutputFields.Scopes))
            {
                _scopeProvider.ForEachScope((scope, nextState) =>
                {
                    nextState.Append("\n => ");
                    nextState.Append(scope);
                }, builder);
            }

            try
            {
                _testOutputHelper.WriteLine(builder.ToString());
            }
            catch (InvalidOperationException)
            {
                // Silently ignore when there is no currently active test.
            }
        }

        private static string GetLogLevelString(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "TRCE",
                LogLevel.Debug => "DBUG",
                LogLevel.Information => "INFO",
                LogLevel.Warning => "WARN",
                LogLevel.Error => "FAIL",
                LogLevel.Critical => "CRIT",
                LogLevel.None => "",
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
            };
        }

        private sealed class NoExternalScopeProvider : IExternalScopeProvider
        {
            public void ForEachScope<TState>(Action<object?, TState> callback, TState state)
            {
            }

            public IDisposable Push(object? state)
            {
                return EmptyDisposable.Instance;
            }

            private sealed class EmptyDisposable : IDisposable
            {
                public static EmptyDisposable Instance { get; } = new();

                public void Dispose()
                {
                }
            }
        }
    }
}
