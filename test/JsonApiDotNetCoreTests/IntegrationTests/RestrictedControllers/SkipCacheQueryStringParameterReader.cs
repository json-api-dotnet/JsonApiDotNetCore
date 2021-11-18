using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.QueryStrings;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers
{
    public sealed class SkipCacheQueryStringParameterReader : IQueryStringParameterReader
    {
        private const string SkipCacheParameterName = "skipCache";

        [UsedImplicitly]
        public bool SkipCache { get; private set; }

        public bool AllowEmptyValue => true;

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
            SkipCache = true;
        }
    }
}
