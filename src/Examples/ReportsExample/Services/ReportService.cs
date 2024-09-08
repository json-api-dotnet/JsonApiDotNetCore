using JetBrains.Annotations;
using JsonApiDotNetCore.Services;
using ReportsExample.Models;

namespace ReportsExample.Services;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class ReportService : IGetAllService<Report, int>
{
    public Task<IReadOnlyCollection<Report>> GetAsync(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Report> reports = GetReports().AsReadOnly();
        return Task.FromResult(reports);
    }

    private List<Report> GetReports()
    {
        return
        [
            new Report
            {
                Id = 1,
                Title = "Status Report",
                Statistics = new ReportStatistics
                {
                    ProgressIndication = "Almost done",
                    HoursSpent = 24
                }
            }
        ];
    }
}
