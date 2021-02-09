using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings
{
    public sealed class AccountPreferences : Identifiable
    {
        [Attr]
        public bool UseDarkTheme { get; set; }
    }
}
