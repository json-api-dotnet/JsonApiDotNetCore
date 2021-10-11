using JetBrains.Annotations;

namespace JsonApiDotNetCore.Controllers.Annotations
{
    /// <summary>
    /// Used on an ASP.NET Core controller class to indicate the POST verb must be blocked.
    /// </summary>
    /// <example><![CDATA[
    /// [NoHttpost]
    /// public class ArticlesController : BaseJsonApiController<Article>
    /// {
    /// }
    /// ]]></example>
    [PublicAPI]
    public sealed class NoHttpPostAttribute : HttpRestrictAttribute
    {
        protected override string[] Methods { get; } =
        {
            "POST"
        };
    }
}
