namespace JsonApiDotNetCore.Controllers
{
    public sealed class HttpReadOnlyAttribute : HttpRestrictAttribute
    {
        protected override string[] Methods { get; } = { "POST", "PATCH", "DELETE" };
    }
}
