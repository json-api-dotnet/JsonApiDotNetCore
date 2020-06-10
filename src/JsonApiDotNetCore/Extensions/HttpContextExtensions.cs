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

        internal static void DisableValidator(this HttpContext httpContext, string model, string name)
        {
            if (httpContext == null) return;
            var itemKey = $"JsonApiDotNetCore_DisableValidation_{model}_{name}";
            if (!httpContext.Items.ContainsKey(itemKey))
            {
                httpContext.Items.Add(itemKey, true);
            }
        }
        internal static bool IsValidatorDisabled(this HttpContext httpContext, string model, string name)
        {
            return httpContext.Items.ContainsKey($"JsonApiDotNetCore_DisableValidation_{model}_{name}") ||
                   httpContext.Items.ContainsKey($"JsonApiDotNetCore_DisableValidation_{model}_Relation");
        }
    }
}
