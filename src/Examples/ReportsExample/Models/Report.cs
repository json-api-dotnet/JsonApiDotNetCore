using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace ReportsExample.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(GenerateControllerEndpoints = JsonApiEndpoints.GetCollection)]
    public sealed class Report : Identifiable<int>
    {
        [Attr]
        public string Title { get; set; } = null!;

        [Attr]
        public ReportStatistics Statistics { get; set; } = null!;
    }
}
