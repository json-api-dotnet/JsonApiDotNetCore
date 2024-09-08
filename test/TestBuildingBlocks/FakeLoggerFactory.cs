using JetBrains.Annotations;
using JsonApiDotNetCore;
using Microsoft.Extensions.Logging;

namespace TestBuildingBlocks;

[PublicAPI]
public sealed class FakeLoggerFactory(LogLevel minimumLevel) : ILoggerFactory, ILoggerProvider
{
    public FakeLogger Logger { get; } = new(minimumLevel);

    public ILogger CreateLogger(string categoryName)
    {
        return Logger;
    }

    public void AddProvider(ILoggerProvider provider)
    {
    }

    public void Dispose()
    {
    }

    public sealed class FakeLogger(LogLevel minimumLevel) : ILogger
    {
        private readonly LogLevel _minimumLevel = minimumLevel;

        private readonly object _lockObject = new();
        private readonly List<FakeLogMessage> _messages = [];

        public bool IsEnabled(LogLevel logLevel)
        {
            return _minimumLevel != LogLevel.None && logLevel >= _minimumLevel;
        }

        public void Clear()
        {
            lock (_lockObject)
            {
                _messages.Clear();
            }
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                string message = formatter(state, exception);

                lock (_lockObject)
                {
                    _messages.Add(new FakeLogMessage(logLevel, message));
                }
            }
        }

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
        {
            return NullScope.Instance;
        }

        public IReadOnlyList<FakeLogMessage> GetMessages()
        {
            lock (_lockObject)
            {
                return _messages.ToArray().AsReadOnly();
            }
        }

        public IReadOnlyList<string> GetLines()
        {
            lock (_lockObject)
            {
                return _messages.Select(message => message.ToString()).ToArray().AsReadOnly();
            }
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
