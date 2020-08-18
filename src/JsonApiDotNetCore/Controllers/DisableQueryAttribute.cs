using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonApiDotNetCore.Controllers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public sealed class DisableQueryAttribute : Attribute
    {
        public IReadOnlyCollection<string> ParameterNames { get; }

        public static readonly DisableQueryAttribute Empty = new DisableQueryAttribute(StandardQueryStringParameters.None);

        /// <summary>
        /// Disables one or more of the builtin query parameters for a controller.
        /// </summary>
        public DisableQueryAttribute(StandardQueryStringParameters parameters)
        {
            var parameterNames = new List<string>();

            foreach (StandardQueryStringParameters value in Enum.GetValues(typeof(StandardQueryStringParameters)))
            {
                if (value != StandardQueryStringParameters.None && value != StandardQueryStringParameters.All &&
                    parameters.HasFlag(value))
                {
                    parameterNames.Add(value.ToString().ToLowerInvariant());
                }
            }

            ParameterNames = parameterNames;
        }

        /// <summary>
        /// It is allowed to use a comma-separated list of strings to indicate which query parameters
        /// should be disabled, because the user may have defined custom query parameters that are
        /// not included in the <see cref="StandardQueryStringParameters"/> enum.
        /// </summary>
        public DisableQueryAttribute(string parameterNames)
        {
            ParameterNames = parameterNames.Split(",").Select(x => x.Trim().ToLowerInvariant()).ToList();
        }

        public bool ContainsParameter(StandardQueryStringParameters parameter)
        {
            var name = parameter.ToString().ToLowerInvariant();
            return ParameterNames.Contains(name);
        }
    }
}
