using JetBrains.Annotations;

namespace ReportsExample.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ReportStatistics
{
    public required string ProgressIndication { get; set; }
    public int HoursSpent { get; set; }
}
