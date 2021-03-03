using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace ReportsExample.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Report : Identifiable
    {
        [Attr]
        public string Title { get; set; }

        [Attr]
        public ReportStatistics Statistics { get; set; }
    }
}
