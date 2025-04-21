namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal interface ISchemaGenerationTraceScope : IDisposable
{
    void TraceSucceeded(string schemaId);
}
