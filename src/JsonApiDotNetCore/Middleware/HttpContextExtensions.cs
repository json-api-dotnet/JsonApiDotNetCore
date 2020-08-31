using System;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Middleware
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Indicates whether the currently executing HTTP request is being handled by JsonApiDotNetCore.
        /// </summary>
        public static bool IsJsonApiRequest(this HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            string value = httpContext.Items["IsJsonApiRequest"] as string;
            return value == bool.TrueString;
        }

        internal static void RegisterJsonApiRequest(this HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            httpContext.Items["IsJsonApiRequest"] = bool.TrueString;
        }

        internal static void DisableValidator(this HttpContext httpContext, string propertyName, string model)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            if (model == null) throw new ArgumentNullException(nameof(model));

            var itemKey = $"JsonApiDotNetCore_DisableValidation_{model}_{propertyName}";
            httpContext.Items[itemKey] = true;
        }

        internal static bool IsValidatorDisabled(this HttpContext httpContext, string propertyName, string model)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            if (model == null) throw new ArgumentNullException(nameof(model));

            return httpContext.Items.ContainsKey($"JsonApiDotNetCore_DisableValidation_{model}_{propertyName}") ||
                   httpContext.Items.ContainsKey($"JsonApiDotNetCore_DisableValidation_{model}_Relation");
        }
    }
}
