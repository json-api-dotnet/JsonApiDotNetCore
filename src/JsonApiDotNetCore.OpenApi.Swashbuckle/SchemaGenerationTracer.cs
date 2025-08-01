using System.Runtime.CompilerServices;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

/// <summary>
/// Enables to log recursive component schema generation at trace level.
/// </summary>
internal sealed partial class SchemaGenerationTracer
{
    private readonly ILoggerFactory _loggerFactory;

    public SchemaGenerationTracer(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _loggerFactory = loggerFactory;
    }

    public ISchemaGenerationTraceScope TraceStart(object generator)
    {
        ArgumentNullException.ThrowIfNull(generator);

        return InnerTraceStart(generator, () => "(none)");
    }

    public ISchemaGenerationTraceScope TraceStart(object generator, Type schemaType)
    {
        ArgumentNullException.ThrowIfNull(generator);
        ArgumentNullException.ThrowIfNull(schemaType);

        return InnerTraceStart(generator, () => GetSchemaTypeName(schemaType));
    }

    public ISchemaGenerationTraceScope TraceStart(object generator, AtomicOperationCode operationCode)
    {
        ArgumentNullException.ThrowIfNull(generator);

        return InnerTraceStart(generator, () => $"{nameof(AtomicOperationCode)}.{operationCode}");
    }

    public ISchemaGenerationTraceScope TraceStart(object generator, RelationshipAttribute relationship)
    {
        ArgumentNullException.ThrowIfNull(generator);
        ArgumentNullException.ThrowIfNull(relationship);

        return InnerTraceStart(generator,
            () => $"{GetSchemaTypeName(relationship.GetType())}({GetSchemaTypeName(relationship.LeftType.ClrType)}.{relationship.Property.Name})");
    }

    public ISchemaGenerationTraceScope TraceStart(object generator, Type schemaOpenType, RelationshipAttribute relationship)
    {
        ArgumentNullException.ThrowIfNull(generator);
        ArgumentNullException.ThrowIfNull(schemaOpenType);
        ArgumentNullException.ThrowIfNull(relationship);

        return InnerTraceStart(generator,
            () =>
                $"{GetSchemaTypeName(schemaOpenType)} with {GetSchemaTypeName(relationship.GetType())}({GetSchemaTypeName(relationship.LeftType.ClrType)}.{relationship.Property.Name})");
    }

    private ISchemaGenerationTraceScope InnerTraceStart(object generator, Func<string> getSchemaTypeName)
    {
        ILogger logger = _loggerFactory.CreateLogger(generator.GetType());

        if (logger.IsEnabled(LogLevel.Trace))
        {
            string schemaTypeName = getSchemaTypeName();
            return new SchemaGenerationTraceScope(logger, schemaTypeName);
        }

        return DisabledSchemaGenerationTraceScope.Instance;
    }

    private static string GetSchemaTypeName(Type type)
    {
        if (type.IsConstructedGenericType)
        {
            string typeArguments = string.Join(',', type.GetGenericArguments().Select(GetSchemaTypeName));
            int arityIndex = type.Name.IndexOf('`');
            return $"{type.Name[..arityIndex]}<{typeArguments}>";
        }

        return type.Name;
    }

    private sealed partial class SchemaGenerationTraceScope : ISchemaGenerationTraceScope
    {
        private static readonly AsyncLocal<StrongBox<int>> RecursionDepthAsyncLocal = new();

        private readonly ILogger _logger;
        private readonly string _schemaTypeName;
        private string? _schemaId;

        public SchemaGenerationTraceScope(ILogger logger, string schemaTypeName)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(schemaTypeName);

            _logger = logger;
            _schemaTypeName = schemaTypeName;

            RecursionDepthAsyncLocal.Value ??= new StrongBox<int>(0);
            int depth = Interlocked.Increment(ref RecursionDepthAsyncLocal.Value.Value);

            LogStarted(depth, _schemaTypeName);
        }

        public void TraceSucceeded(string schemaId)
        {
            _schemaId = schemaId;
        }

        public void Dispose()
        {
            int depth = RecursionDepthAsyncLocal.Value!.Value;

            if (_schemaId != null)
            {
                LogSucceeded(depth, _schemaTypeName, _schemaId);
            }
            else
            {
                LogFailed(depth, _schemaTypeName);
            }

            Interlocked.Decrement(ref RecursionDepthAsyncLocal.Value.Value);
        }

        [LoggerMessage(Level = LogLevel.Trace, SkipEnabledCheck = true, Message = "({Depth:D2}) Started for {SchemaTypeName}.")]
        private partial void LogStarted(int depth, string schemaTypeName);

        [LoggerMessage(Level = LogLevel.Trace, SkipEnabledCheck = true, Message = "({Depth:D2}) Generated '{SchemaId}' from {SchemaTypeName}.")]
        private partial void LogSucceeded(int depth, string schemaTypeName, string schemaId);

        [LoggerMessage(Level = LogLevel.Trace, SkipEnabledCheck = true, Message = "({Depth:D2}) Failed for {SchemaTypeName}.")]
        private partial void LogFailed(int depth, string schemaTypeName);
    }

    private sealed class DisabledSchemaGenerationTraceScope : ISchemaGenerationTraceScope
    {
        public static DisabledSchemaGenerationTraceScope Instance { get; } = new();

        private DisabledSchemaGenerationTraceScope()
        {
        }

        public void TraceSucceeded(string schemaId)
        {
        }

        public void Dispose()
        {
        }
    }
}
