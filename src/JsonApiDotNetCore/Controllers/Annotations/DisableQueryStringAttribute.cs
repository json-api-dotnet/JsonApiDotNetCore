using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.QueryStrings;

namespace JsonApiDotNetCore.Controllers.Annotations
{
    /// <summary>
    /// Used on an ASP.NET Core controller class to indicate which query string parameters are blocked.
    /// </summary>
    /// <example><![CDATA[
    /// [DisableQueryString(JsonApiQueryStringParameters.Sort | JsonApiQueryStringParameters.Page)]
    /// public class CustomersController : JsonApiController<Customer> { }
    /// ]]></example>
    /// <example><![CDATA[
    /// [DisableQueryString("skipCache")]
    /// public class CustomersController : JsonApiController<Customer> { }
    /// ]]></example>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public sealed class DisableQueryStringAttribute : Attribute
    {
        public static readonly DisableQueryStringAttribute Empty = new(JsonApiQueryStringParameters.None);

        public IReadOnlySet<string> ParameterNames { get; }

        /// <summary>
        /// Disables one or more of the builtin query parameters for a controller.
        /// </summary>
        public DisableQueryStringAttribute(JsonApiQueryStringParameters parameters)
        {
            var parameterNames = new HashSet<string>();

            foreach (JsonApiQueryStringParameters value in Enum.GetValues(typeof(JsonApiQueryStringParameters)))
            {
                if (value != JsonApiQueryStringParameters.None && value != JsonApiQueryStringParameters.All && parameters.HasFlag(value))
                {
                    parameterNames.Add(value.ToString());
                }
            }

            ParameterNames = parameterNames;
        }

        /// <summary>
        /// It is allowed to use a comma-separated list of strings to indicate which query parameters should be disabled, because the user may have defined
        /// custom query parameters that are not included in the <see cref="JsonApiQueryStringParameters" /> enum.
        /// </summary>
        public DisableQueryStringAttribute(string parameterNames)
        {
            ArgumentGuard.NotNullNorEmpty(parameterNames, nameof(parameterNames));

            ParameterNames = parameterNames.Split(",").ToHashSet();
        }

        public bool ContainsParameter(JsonApiQueryStringParameters parameter)
        {
            string name = parameter.ToString();
            return ParameterNames.Contains(name);
        }
    }
}
