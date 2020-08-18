using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Middleware
{
    public static class HttpContextExtensions
    {
        public static bool IsJsonApiRequest(this HttpContext httpContext)
        {
            string value = httpContext.Items["IsJsonApiRequest"] as string;
            return value == bool.TrueString;
        }

        internal static void RegisterJsonApiRequest(this HttpContext httpContext)
        {
            httpContext.Items["IsJsonApiRequest"] = bool.TrueString;
        }

        internal static void DisableValidator(this HttpContext httpContext, string propertyName, string model)
        {
            var itemKey = $"JsonApiDotNetCore_DisableValidation_{model}_{propertyName}";
            httpContext.Items[itemKey] = true;
        }

        internal static bool IsValidatorDisabled(this HttpContext httpContext, string propertyName, string model)
        {
            return httpContext.Items.ContainsKey($"JsonApiDotNetCore_DisableValidation_{model}_{propertyName}") ||
                   httpContext.Items.ContainsKey($"JsonApiDotNetCore_DisableValidation_{model}_Relation");
        }
    }
}
