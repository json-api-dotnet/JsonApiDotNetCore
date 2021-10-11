using JetBrains.Annotations;

namespace JsonApiDotNetCore.Controllers.Annotations
{
    /// <summary>
    /// Used on an ASP.NET Core controller class to indicate the PATCH verb must be blocked.
    /// </summary>
    /// <example><![CDATA[
    /// [NoHttpPatch]
    /// public class ArticlesController : BaseJsonApiController<Article>
    /// {
    /// }
    /// ]]></example>
    [PublicAPI]
    public sealed class NoHttpPatchAttribute : HttpRestrictAttribute
    {
        protected override string[] Methods { get; } =
        {
            "PATCH"
        };
    }
}
