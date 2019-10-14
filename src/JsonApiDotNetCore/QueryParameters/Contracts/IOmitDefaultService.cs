namespace JsonApiDotNetCore.Query
{
    public interface IOmitDefaultService : IParsableQueryParameter
    {
        bool Config { get; }
    }
}