using System.Linq;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Query;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCoreExample.Services
{
    public class SkipCacheQueryParameterService : IQueryParameterService
    {
        private const string _skipCacheParameterName = "skipCache";

        public bool SkipCache { get; private set; }

        public bool IsEnabled(DisableQueryAttribute disableQueryAttribute)
        {
            return !disableQueryAttribute.ParameterNames.Contains(_skipCacheParameterName.ToLowerInvariant());
        }

        public bool CanParse(string parameterName)
        {
            return parameterName == _skipCacheParameterName;
        }

        public void Parse(string parameterName, StringValues parameterValue)
        {
            if (!bool.TryParse(parameterValue, out bool skipCache))
            {
                throw new InvalidQueryStringParameterException(parameterName, "Boolean value required.",
                    $"The value {parameterValue} is not a valid boolean.");
            }

            SkipCache = skipCache;
        }
    }
}
