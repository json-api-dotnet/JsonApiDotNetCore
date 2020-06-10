using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Extensions
{
    public static class HttpContextExtensions
    {
        public static bool IsJsonApiRequest(this HttpContext httpContext)
        {
            string value = httpContext.Items["IsJsonApiRequest"] as string;
            return value == bool.TrueString;
        }

        internal static void SetJsonApiRequest(this HttpContext httpContext)
        {
            httpContext.Items["IsJsonApiRequest"] = bool.TrueString;
        }

        internal static void DisableValidator(this HttpContext httpContext, string propertyName, string model = null)
        {
            if (httpContext == null) return;
            var itemKey = $"JsonApiDotNetCore_DisableValidation_{model}_{propertyName}";
            if (!httpContext.Items.ContainsKey(itemKey) && model != null)
            {
                httpContext.Items.Add(itemKey, true);
            }
        }
        internal static bool IsValidatorDisabled(this HttpContext httpContext, string propertyName, string model)
        {
            return httpContext.Items.ContainsKey($"JsonApiDotNetCore_DisableValidation_{model}_{propertyName}") ||
                   httpContext.Items.ContainsKey($"JsonApiDotNetCore_DisableValidation_{model}_Relation");
        }
    }
}
