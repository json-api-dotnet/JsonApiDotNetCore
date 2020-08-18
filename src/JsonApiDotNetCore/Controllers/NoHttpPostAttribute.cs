namespace JsonApiDotNetCore.Controllers
{
    public sealed class NoHttpPostAttribute : HttpRestrictAttribute
    {
        protected override string[] Methods { get; } = { "POST" };
    }
}
