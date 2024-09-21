using JetBrains.Annotations;
using JsonApiDotNetCore;
using Microsoft.Extensions.Logging;

namespace TestBuildingBlocks;

[PublicAPI]
public sealed class CapturingLoggerProvider : ILoggerProvider
{
    private static readonly Func<string, LogLevel, bool> DefaultFilter = (_, _) => true;
    private readonly Func<string, LogLevel, bool> _filter;

    private readonly object _lockObject = new();
    private readonly List<LogMessage> _messages = [];

    public CapturingLoggerProvider()
        : this(DefaultFilter)
    {
    }

    public CapturingLoggerProvider(LogLevel minimumLevel)
        : this((_, logLevel) => logLevel != LogLevel.None && logLevel >= minimumLevel)
    {
    }

    public CapturingLoggerProvider(Func<string, LogLevel, bool> filter)
    {
        ArgumentGuard.NotNull(filter);

        _filter = filter;
    }

    public ILogger CreateLogger(string categoryName)
    {
        ArgumentGuard.NotNullNorEmpty(categoryName);

        return new CapturingLogger(this, categoryName, _filter);
    }

    public void Clear()
    {
        lock (_lockObject)
        {
            _messages.Clear();
        }
    }

    public IReadOnlyList<LogMessage> GetMessages()
    {
        LogMessage[] snapshot;

        lock (_lockObject)
        {
            snapshot = _messages.ToArray();
        }

        return snapshot.AsReadOnly();
    }

    public IReadOnlyList<string> GetLines()
    {
        IReadOnlyList<LogMessage> snapshot = GetMessages();
        return snapshot.Select(message => message.ToString()).ToArray().AsReadOnly();
    }

    private void Add(LogMessage message)
    {
        lock (_lockObject)
        {
            _messages.Add(message);
        }
    }

    public void Dispose()
    {
    }

    private sealed class CapturingLogger(CapturingLoggerProvider owner, string categoryName, Func<string, LogLevel, bool> filter) : ILogger
    {
        private readonly CapturingLoggerProvider _owner = owner;
        private readonly string _categoryName = categoryName;
        private readonly Func<string, LogLevel, bool> _filter = filter;

        public bool IsEnabled(LogLevel logLevel)
        {
            return _filter(_categoryName, logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                string text = formatter(state, exception);
                var message = new LogMessage(logLevel, _categoryName, text);
                _owner.Add(message);
            }
        }

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
        {
            return NullScope.Instance;
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            private NullScope()
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
