using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Middleware
{
    internal abstract class TraceLogWriter
    {
        protected static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            ReferenceHandler = ReferenceHandler.Preserve
        };
    }

    internal sealed class TraceLogWriter<T> : TraceLogWriter
    {
        private readonly ILogger _logger;

        private bool IsEnabled => _logger.IsEnabled(LogLevel.Trace);

        public TraceLogWriter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(typeof(T));
        }

        public void LogMethodStart(object? parameters = null, [CallerMemberName] string memberName = "")
        {
            if (IsEnabled)
            {
                string message = FormatMessage(memberName, parameters);
                WriteMessageToLog(message);
            }
        }

        public void LogMessage(Func<string> messageFactory)
        {
            if (IsEnabled)
            {
                string message = messageFactory();
                WriteMessageToLog(message);
            }
        }

        private static string FormatMessage(string memberName, object? parameters)
        {
            var builder = new StringBuilder();

            builder.Append("Entering ");
            builder.Append(memberName);
            builder.Append('(');
            WriteProperties(builder, parameters);
            builder.Append(')');

            return builder.ToString();
        }

        private static void WriteProperties(StringBuilder builder, object? propertyContainer)
        {
            if (propertyContainer != null)
            {
                bool isFirstMember = true;

                foreach (PropertyInfo property in propertyContainer.GetType().GetProperties())
                {
                    if (isFirstMember)
                    {
                        isFirstMember = false;
                    }
                    else
                    {
                        builder.Append(", ");
                    }

                    WriteProperty(builder, property, propertyContainer);
                }
            }
        }

        private static void WriteProperty(StringBuilder builder, PropertyInfo property, object instance)
        {
            builder.Append(property.Name);
            builder.Append(": ");

            object? value = property.GetValue(instance);

            if (value == null)
            {
                builder.Append("null");
            }
            else if (value is string stringValue)
            {
                builder.Append('"');
                builder.Append(stringValue);
                builder.Append('"');
            }
            else
            {
                WriteObject(builder, value);
            }
        }

        private static void WriteObject(StringBuilder builder, object value)
        {
            if (HasToStringOverload(value.GetType()))
            {
                builder.Append(value);
            }
            else
            {
                string text = SerializeObject(value);
                builder.Append(text);
            }
        }

        private static bool HasToStringOverload(Type? type)
        {
            if (type != null)
            {
                MethodInfo? toStringMethod = type.GetMethod("ToString", Array.Empty<Type>());

                if (toStringMethod != null && toStringMethod.DeclaringType != typeof(object))
                {
                    return true;
                }
            }

            return false;
        }

        private static string SerializeObject(object value)
        {
            try
            {
                return JsonSerializer.Serialize(value, SerializerOptions);
            }
            catch (JsonException)
            {
                // Never crash as a result of logging, this is best-effort only.
                return "object";
            }
        }

        private void WriteMessageToLog(string message)
        {
            _logger.LogTrace(message);
        }
    }
}
