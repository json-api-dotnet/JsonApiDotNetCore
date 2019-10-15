namespace JsonApiDotNetCore.Query
{
    public interface IOmitNullService : IQueryParameterService
    {
        bool Config { get; }
    }
}