namespace JsonApiDotNetCore.Query
{
    public interface IOmitDefaultService : IQueryParameterService
    {
        bool Config { get; }
    }
}