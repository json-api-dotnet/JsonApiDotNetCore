using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Middleware;

internal abstract class TraceLogWriter
{
    protected static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(),
            new ResourceTypeInTraceJsonConverter(),
            new ResourceFieldInTraceJsonConverterFactory(),
            new AbstractResourceWrapperInTraceJsonConverterFactory(),
            new IdentifiableInTraceJsonConverter()
        }
    };

    private sealed class ResourceTypeInTraceJsonConverter : JsonConverter<ResourceType>
    {
        public override ResourceType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public override void Write(Utf8JsonWriter writer, ResourceType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.PublicName);
        }
    }

    private sealed class ResourceFieldInTraceJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsAssignableTo(typeof(ResourceFieldAttribute));
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Type converterType = typeof(ResourceFieldInTraceJsonConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }

        private sealed class ResourceFieldInTraceJsonConverter<TField> : JsonConverter<TField>
            where TField : ResourceFieldAttribute
        {
            public override TField Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new NotSupportedException();
            }

            public override void Write(Utf8JsonWriter writer, TField value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.PublicName);
            }
        }
    }

    private sealed class IdentifiableInTraceJsonConverter : JsonConverter<IIdentifiable>
    {
        public override IIdentifiable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public override void Write(Utf8JsonWriter writer, IIdentifiable value, JsonSerializerOptions options)
        {
            // Intentionally *not* calling GetClrType() because we need delegation to the wrapper converter.
            Type runtimeType = value.GetType();

            JsonSerializer.Serialize(writer, value, runtimeType, options);
        }
    }

    private sealed class AbstractResourceWrapperInTraceJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsAssignableTo(typeof(IAbstractResourceWrapper));
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Type converterType = typeof(AbstractResourceWrapperInTraceJsonConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }

        private sealed class AbstractResourceWrapperInTraceJsonConverter<TWrapper> : JsonConverter<TWrapper>
            where TWrapper : IAbstractResourceWrapper
        {
            public override TWrapper Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new NotSupportedException();
            }

            public override void Write(Utf8JsonWriter writer, TWrapper value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WriteString("ClrType", value.AbstractType.FullName);
                writer.WriteString("StringId", value.StringId);
                writer.WriteEndObject();
            }
        }
    }
}

internal sealed partial class TraceLogWriter<T>(ILoggerFactory loggerFactory) : TraceLogWriter
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<T>();

    public void LogMethodStart(object? parameters = null, [CallerMemberName] string memberName = "")
    {
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            var builder = new StringBuilder();
            WriteProperties(builder, parameters);
            string parameterValues = builder.ToString();

            if (parameterValues.Length == 0)
            {
                LogEnteringMember(memberName);
            }
            else
            {
                LogEnteringMemberWithParameters(memberName, parameterValues);
            }
        }
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
        WriteObject(builder, value);
    }

    private static void WriteObject(StringBuilder builder, object? value)
    {
        if (value != null && value is not string && HasToStringOverload(value.GetType()))
        {
            builder.Append(value);
        }
        else
        {
            string text = SerializeObject(value);
            builder.Append(text);
        }
    }

    private static bool HasToStringOverload(Type type)
    {
        MethodInfo? toStringMethod = type.GetMethod("ToString", []);
        return toStringMethod != null && toStringMethod.DeclaringType != typeof(object);
    }

    private static string SerializeObject(object? value)
    {
        try
        {
            return JsonSerializer.Serialize(value, SerializerOptions);
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException)
        {
            // Never crash as a result of logging, this is best-effort only.
            return "object";
        }
    }

    public void LogDebug(QueryLayer queryLayer)
    {
        ArgumentNullException.ThrowIfNull(queryLayer);

        LogQueryLayer(queryLayer);
    }

    public void LogDebug(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            string? text = ExpressionTreeFormatter.Instance.GetText(expression);

            if (text != null)
            {
                LogExpression(text);
            }
            else
            {
                LogReadableExpressionsAssemblyUnavailable();
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Trace, SkipEnabledCheck = true, Message = "Entering {MemberName}({ParameterValues})")]
    private partial void LogEnteringMemberWithParameters(string memberName, string parameterValues);

    [LoggerMessage(Level = LogLevel.Trace, SkipEnabledCheck = true, Message = "Entering {MemberName}()")]
    private partial void LogEnteringMember(string memberName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "QueryLayer: {queryLayer}")]
    private partial void LogQueryLayer(QueryLayer queryLayer);

    [LoggerMessage(Level = LogLevel.Debug, SkipEnabledCheck = true, Message = "Expression tree: {expression}")]
    private partial void LogExpression(string expression);

    [LoggerMessage(Level = LogLevel.Debug, SkipEnabledCheck = true,
        Message = "Failed to load assembly. To log expression trees, add a NuGet reference to 'AgileObjects.ReadableExpressions' in your project.")]
    private partial void LogReadableExpressionsAssemblyUnavailable();
}
