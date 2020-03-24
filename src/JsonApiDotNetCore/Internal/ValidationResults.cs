using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Internal
{
    public sealed class ValidationResult
    {
        public ValidationResult(LogLevel logLevel, string message)
        {
            LogLevel = logLevel;
            Message = message;
        }

        public LogLevel LogLevel { get; }
        public string Message { get; }
    }
}
