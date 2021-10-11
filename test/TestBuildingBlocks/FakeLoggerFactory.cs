#nullable disable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace TestBuildingBlocks
{
    [PublicAPI]
    public sealed class FakeLoggerFactory : ILoggerFactory, ILoggerProvider
    {
        public FakeLogger Logger { get; }

        public FakeLoggerFactory(LogLevel minimumLevel)
        {
            Logger = new FakeLogger(minimumLevel);
        }

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

        public sealed class FakeLogger : ILogger
        {
            private readonly LogLevel _minimumLevel;
            private readonly ConcurrentBag<FakeLogMessage> _messages = new();

            public IReadOnlyCollection<FakeLogMessage> Messages => _messages;

            public FakeLogger(LogLevel minimumLevel)
            {
                _minimumLevel = minimumLevel;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return _minimumLevel != LogLevel.None && logLevel >= _minimumLevel;
            }

            public void Clear()
            {
                _messages.Clear();
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (IsEnabled(logLevel))
                {
                    string message = formatter(state, exception);
                    _messages.Add(new FakeLogMessage(logLevel, message));
                }
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }

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
    }
}
