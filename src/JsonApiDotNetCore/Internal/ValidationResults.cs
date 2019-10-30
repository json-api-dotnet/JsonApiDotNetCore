using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Internal
{
    public class ValidationResult
    {
        public ValidationResult(LogLevel logLevel, string message)
        {
            LogLevel = logLevel;
            Message = message;
        }

        public LogLevel LogLevel { get; set; }
        public string Message { get; set; }
    }
}
