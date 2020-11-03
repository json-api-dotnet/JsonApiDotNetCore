using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests
{
    internal sealed class FakeLoggerFactory : ILoggerFactory, ILoggerProvider
    {
        public FakeLogger Logger { get; }

        public FakeLoggerFactory()
        {
            Logger = new FakeLogger();
        }

        public ILogger CreateLogger(string categoryName) => Logger;

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public void Dispose()
        {
        }

        internal sealed class FakeLogger : ILogger
        {
            private readonly ConcurrentBag<LogMessage> _messages = new ConcurrentBag<LogMessage>();

            public IReadOnlyCollection<LogMessage> Messages => _messages;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Clear()
            {
                _messages.Clear();
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter)
            {
                var message = formatter(state, exception);
                _messages.Add(new LogMessage(logLevel, message));
            }

            public IDisposable BeginScope<TState>(TState state) => null;
        }

        internal sealed class LogMessage
        {
            public LogLevel LogLevel { get; }
            public string Text { get; }

            public LogMessage(LogLevel logLevel, string text)
            {
                LogLevel = logLevel;
                Text = text;
            }
        }
    }
}
