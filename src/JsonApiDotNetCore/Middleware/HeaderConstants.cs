namespace JsonApiDotNetCore.Middleware
{
    public static class HeaderConstants
    {
        public const string MediaType = "application/vnd.api+json";
        public const string AtomicOperationsMediaType = MediaType + "; ext=\"https://jsonapi.org/ext/atomic\"";
    }
}
