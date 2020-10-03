using System;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Middleware
{
    public static class HttpContextExtensions
    {
        private const string _isJsonApiRequestKey = "JsonApiDotNetCore_IsJsonApiRequest";
        private const string _disableRequiredValidatorKey = "JsonApiDotNetCore_DisableRequiredValidator";

        /// <summary>
        /// Indicates whether the currently executing HTTP request is being handled by JsonApiDotNetCore.
        /// </summary>
        public static bool IsJsonApiRequest(this HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            string value = httpContext.Items[_isJsonApiRequestKey] as string;
            return value == bool.TrueString;
        }

        internal static void RegisterJsonApiRequest(this HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            httpContext.Items[_isJsonApiRequestKey] = bool.TrueString;
        }

        internal static void DisableRequiredValidator(this HttpContext httpContext, string propertyName, string model)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            if (model == null) throw new ArgumentNullException(nameof(model));

            var itemKey = $"{_disableRequiredValidatorKey}_{model}_{propertyName}";
            httpContext.Items[itemKey] = true;
        }

        internal static bool IsRequiredValidatorDisabled(this HttpContext httpContext, string propertyName, string model)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            if (model == null) throw new ArgumentNullException(nameof(model));

            return httpContext.Items.ContainsKey($"{_disableRequiredValidatorKey}_{model}_{propertyName}");
        }
    }
}
