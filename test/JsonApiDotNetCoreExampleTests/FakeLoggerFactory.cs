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
            private readonly ConcurrentBag<(LogLevel LogLevel, string Text)> _messages = new ConcurrentBag<(LogLevel LogLevel, string Text)>();

            public IReadOnlyCollection<(LogLevel LogLevel, string Text)> Messages => _messages;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Clear()
            {
                _messages.Clear();
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter)
            {
                var message = formatter(state, exception);
                _messages.Add((logLevel, message));
            }

            public IDisposable BeginScope<TState>(TState state) => null;
        }
    }
}
