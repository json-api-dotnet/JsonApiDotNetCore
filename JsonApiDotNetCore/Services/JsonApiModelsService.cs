namespace JsonApiDotNetCore.Services
{
    public class JsonApiService
    {
        private JsonApiModelConfiguration _jsonApiModelConfiguration;

        public JsonApiService(JsonApiModelConfiguration configuration)
        {
            _jsonApiModelConfiguration = configuration;
        }

        public void HandleJsonApiRoute(string route)
        {
            var modelType = _jsonApiModelConfiguration.GetTypeForRoute(route);
            // TODO:
        }
    }
}
