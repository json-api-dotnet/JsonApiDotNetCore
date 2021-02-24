using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.QueryStrings;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RestrictedControllers
{
    public sealed class SkipCacheQueryStringParameterReader : IQueryStringParameterReader
    {
        private const string SkipCacheParameterName = "skipCache";

        [UsedImplicitly]
        public bool SkipCache { get; private set; }

        public bool IsEnabled(DisableQueryStringAttribute disableQueryStringAttribute)
        {
            return !disableQueryStringAttribute.ParameterNames.Contains(SkipCacheParameterName);
        }

        public bool CanRead(string parameterName)
        {
            return parameterName == SkipCacheParameterName;
        }

        public void Read(string parameterName, StringValues parameterValue)
        {
            if (!bool.TryParse(parameterValue, out bool skipCache))
            {
                throw new InvalidQueryStringParameterException(parameterName, "Boolean value required.",
                    $"The value '{parameterValue}' is not a valid boolean.");
            }

            SkipCache = skipCache;
        }
    }
}
