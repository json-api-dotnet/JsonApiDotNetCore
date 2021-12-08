using JetBrains.Annotations;

namespace ReportsExample.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ReportStatistics
{
    public string ProgressIndication { get; set; } = null!;
    public int HoursSpent { get; set; }
}
