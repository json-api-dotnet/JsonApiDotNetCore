using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

/// <summary>
/// Enables to log recursive component schema generation at trace level.
/// </summary>
internal sealed partial class SchemaGenerationTracer
{
    private static readonly AsyncLocal<int> RecursionDepthAsyncLocal = new();
    private readonly ILoggerFactory _loggerFactory;

    public SchemaGenerationTracer(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _loggerFactory = loggerFactory;
    }

    public IDisposable TraceStart(object generator, string schemaId)
    {
        ArgumentNullException.ThrowIfNull(generator);
        ArgumentException.ThrowIfNullOrEmpty(schemaId);

        ILogger logger = _loggerFactory.CreateLogger(generator.GetType());

        if (logger.IsEnabled(LogLevel.Trace))
        {
            return new SchemaGenerationTraceScope(logger, schemaId);
        }

        return DisabledTraceScope.Instance;
    }

    public IDisposable TraceStart(object generator, Type schemaType)
    {
        ArgumentNullException.ThrowIfNull(generator);
        ArgumentNullException.ThrowIfNull(schemaType);

        ILogger logger = _loggerFactory.CreateLogger(generator.GetType());

        if (logger.IsEnabled(LogLevel.Trace))
        {
            string schemaName = GetSchemaTypeName(schemaType);
            return new SchemaGenerationTraceScope(logger, schemaName);
        }

        return DisabledTraceScope.Instance;
    }

    private static string GetSchemaTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            string typeArguments = string.Join(',', type.GetGenericArguments().Select(GetSchemaTypeName));
            int arityIndex = type.Name.IndexOf('`');
            return $"{type.Name[..arityIndex]}<{typeArguments}>";
        }

        return type.Name;
    }

    private sealed partial class SchemaGenerationTraceScope : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _schemaName;

        public SchemaGenerationTraceScope(ILogger logger, string schemaName)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(schemaName);

            _logger = logger;
            _schemaName = schemaName;

            RecursionDepthAsyncLocal.Value++;
            LogStarted(RecursionDepthAsyncLocal.Value, schemaName);
        }

        public void Dispose()
        {
            // TODO: Keep this?
            //LogCompleted(RecursionDepthAsyncLocal.Value, _schemaName);
            RecursionDepthAsyncLocal.Value--;
        }

        [LoggerMessage(Level = LogLevel.Trace, SkipEnabledCheck = true, Message = "({Depth:D2}) Started for '{SchemaName}'.")]
        private partial void LogStarted(int depth, string schemaName);

        [LoggerMessage(Level = LogLevel.Trace, SkipEnabledCheck = true, Message = "({Depth:D2}) Completed for '{SchemaName}'.")]
        private partial void LogCompleted(int depth, string schemaName);
    }

    private sealed class DisabledTraceScope : IDisposable
    {
        public static DisabledTraceScope Instance { get; } = new();

        private DisabledTraceScope()
        {
        }

        public void Dispose()
        {
        }
    }
}
