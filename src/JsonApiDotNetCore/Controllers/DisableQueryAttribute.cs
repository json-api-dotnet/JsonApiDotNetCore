using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonApiDotNetCore.Controllers
{
    public sealed class DisableQueryAttribute : Attribute
    {
        private readonly List<string> _parameterNames;

        public IReadOnlyCollection<string> ParameterNames => _parameterNames.AsReadOnly();

        public static readonly DisableQueryAttribute Empty = new DisableQueryAttribute(StandardQueryStringParameters.None);

        /// <summary>
        /// Disables one or more of the builtin query parameters for a controller.
        /// </summary>
        public DisableQueryAttribute(StandardQueryStringParameters parameters)
        {
            _parameterNames = parameters != StandardQueryStringParameters.None
                ? ParseList(parameters.ToString())
                : new List<string>();
        }

        /// <summary>
        /// It is allowed to use a comma-separated list of strings to indicate which query parameters
        /// should be disabled, because the user may have defined custom query parameters that are
        /// not included in the <see cref="StandardQueryStringParameters"/> enum.
        /// </summary>
        public DisableQueryAttribute(string parameterNames)
        {
            _parameterNames = ParseList(parameterNames);
        }

        private static List<string> ParseList(string parameterNames)
        {
            return parameterNames.Split(",").Select(x => x.Trim().ToLowerInvariant()).ToList();
        }

        public bool ContainsParameter(StandardQueryStringParameters parameter)
        {
            var name = parameter.ToString().ToLowerInvariant();
            return _parameterNames.Contains(name);
        }
    }
}
