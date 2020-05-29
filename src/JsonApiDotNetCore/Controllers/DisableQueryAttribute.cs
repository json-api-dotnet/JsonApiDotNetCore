using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonApiDotNetCore.Controllers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
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
            _parameterNames = new List<string>();

            foreach (StandardQueryStringParameters value in Enum.GetValues(typeof(StandardQueryStringParameters)))
            {
                if (value != StandardQueryStringParameters.None && value != StandardQueryStringParameters.All &&
                    parameters.HasFlag(value))
                {
                    _parameterNames.Add(value.ToString().ToLowerInvariant());
                }
            }
        }

        /// <summary>
        /// It is allowed to use a comma-separated list of strings to indicate which query parameters
        /// should be disabled, because the user may have defined custom query parameters that are
        /// not included in the <see cref="StandardQueryStringParameters"/> enum.
        /// </summary>
        public DisableQueryAttribute(string parameterNames)
        {
            _parameterNames = parameterNames.Split(",").Select(x => x.Trim().ToLowerInvariant()).ToList();
        }

        public bool ContainsParameter(StandardQueryStringParameters parameter)
        {
            var name = parameter.ToString().ToLowerInvariant();
            return _parameterNames.Contains(name);
        }
    }
}
