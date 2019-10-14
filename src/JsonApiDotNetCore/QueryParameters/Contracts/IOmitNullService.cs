namespace JsonApiDotNetCore.Query
{
    public interface IOmitNullService : IParsableQueryParameter
    {
        bool Config { get; }
    }
}