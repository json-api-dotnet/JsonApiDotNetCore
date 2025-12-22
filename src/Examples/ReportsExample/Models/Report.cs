using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace ReportsExample.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(GenerateControllerEndpoints = JsonApiEndpoints.GetCollection)]
public sealed class Report : Identifiable<long>
{
    [Attr]
    public required string Title { get; set; }

    [Attr]
    public required ReportStatistics Statistics { get; set; }
}
