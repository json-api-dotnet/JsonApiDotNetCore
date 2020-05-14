using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace UnitTests
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
            public List<(LogLevel LogLevel, string Text)> Messages = new List<(LogLevel, string)>();

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter)
            {
                var message = formatter(state, exception);
                Messages.Add((logLevel, message));
            }

            public bool IsEnabled(LogLevel logLevel) => true;
            public IDisposable BeginScope<TState>(TState state) => null;
        }
    }
}
