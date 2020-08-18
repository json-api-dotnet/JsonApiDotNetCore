namespace JsonApiDotNetCore.Controllers
{
    public sealed class NoHttpPatchAttribute : HttpRestrictAttribute
    {
        protected override string[] Methods { get; } = { "PATCH" };
    }
}
