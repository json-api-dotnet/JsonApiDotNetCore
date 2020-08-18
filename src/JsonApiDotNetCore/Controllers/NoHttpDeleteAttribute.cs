namespace JsonApiDotNetCore.Controllers
{
    public sealed class NoHttpDeleteAttribute : HttpRestrictAttribute
    {
        protected override string[] Methods { get; } = { "DELETE" };
    }
}
