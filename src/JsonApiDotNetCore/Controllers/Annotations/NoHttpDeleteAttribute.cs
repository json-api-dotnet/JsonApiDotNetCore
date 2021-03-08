using JetBrains.Annotations;

namespace JsonApiDotNetCore.Controllers.Annotations
{
    /// <summary>
    /// Used on an ASP.NET Core controller class to indicate the DELETE verb must be blocked.
    /// </summary>
    /// <example><![CDATA[
    /// [NoHttpDelete]
    /// public class ArticlesController : BaseJsonApiController<Article>
    /// {
    /// }
    /// ]]></example>
    [PublicAPI]
    public sealed class NoHttpDeleteAttribute : HttpRestrictAttribute
    {
        protected override string[] Methods { get; } =
        {
            "DELETE"
        };
    }
}
