using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace TestBuildingBlocks;

// Based on https://www.meziantou.net/how-to-get-asp-net-core-logs-in-the-output-of-xunit-tests.htm.
public sealed class XUnitLoggerProvider : ILoggerProvider
{
    private const LogOutputFields DefaultLogOutputFields = LogOutputFields.All & ~LogOutputFields.CategoryNamespace;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly LogOutputFields _outputFields;
    private readonly string? _categoryPrefixFilter;

    public XUnitLoggerProvider(ITestOutputHelper testOutputHelper, string? categoryPrefixFilter, LogOutputFields outputFields = DefaultLogOutputFields)
    {
        ArgumentNullException.ThrowIfNull(testOutputHelper);

        _testOutputHelper = testOutputHelper;
        _categoryPrefixFilter = categoryPrefixFilter;
        _outputFields = outputFields;
    }

    public ILogger CreateLogger(string categoryName)
    {
        ArgumentException.ThrowIfNullOrEmpty(categoryName);

        if (_categoryPrefixFilter == null || categoryName.StartsWith(_categoryPrefixFilter, StringComparison.Ordinal))
        {
            return new XUnitLogger(_testOutputHelper, _outputFields, categoryName);
        }

        return NullLogger.Instance;
    }

    public void Dispose()
    {
    }

    private sealed class XUnitLogger(ITestOutputHelper testOutputHelper, LogOutputFields outputFields, string categoryName) : ILogger
    {
        private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;
        private readonly LogOutputFields _outputFields = outputFields;
        private readonly string? _categoryText = GetCategoryText(categoryName, outputFields);

        private static string? GetCategoryText(string categoryName, LogOutputFields outputFields)
        {
            if (outputFields.HasFlag(LogOutputFields.Category))
            {
                return categoryName;
            }

            bool hasName = outputFields.HasFlag(LogOutputFields.CategoryName);
            bool hasNamespace = outputFields.HasFlag(LogOutputFields.CategoryNamespace);

            if (hasName || hasNamespace)
            {
                // Microsoft.Extensions.Logging.LoggerFactory.CreateLogger(Type) removes generic type parameters
                // and replaces '+' (nested class) with '.'.
                int lastDotIndex = categoryName.LastIndexOf('.');

                if (lastDotIndex == -1)
                {
                    return hasName ? categoryName : string.Empty;
                }

                return hasName ? categoryName[(lastDotIndex + 1)..] : categoryName[..lastDotIndex];
            }

            return null;
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
            return EmptyDisposable.Instance;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var builder = new StringBuilder();

            if (_outputFields.HasFlag(LogOutputFields.Level))
            {
                string logLevelString = GetLogLevelString(logLevel);
                builder.Append(logLevelString);
            }

            if (_categoryText != null)
            {
                if (builder.Length > 0)
                {
                    builder.Append(' ');
                }

                builder.Append('[');
                builder.Append(_categoryText);
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
                builder.Append(Environment.NewLine);
                builder.Append(exception);
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

        private sealed class EmptyDisposable : IDisposable
        {
            public static EmptyDisposable Instance { get; } = new();

            public void Dispose()
            {
            }
        }
    }
}
