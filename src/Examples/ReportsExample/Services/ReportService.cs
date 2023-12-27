using JetBrains.Annotations;
using JsonApiDotNetCore.Services;
using ReportsExample.Models;

namespace ReportsExample.Services;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class ReportService(ILoggerFactory loggerFactory) : IGetAllService<Report, int>
{
    private readonly ILogger<ReportService> _logger = loggerFactory.CreateLogger<ReportService>();

    public Task<IReadOnlyCollection<Report>> GetAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetAsync");

        IReadOnlyCollection<Report> reports = GetReports();

        return Task.FromResult(reports);
    }

    private IReadOnlyCollection<Report> GetReports()
    {
        return new List<Report>
        {
            new()
            {
                Id = 1,
                Title = "Status Report",
                Statistics = new ReportStatistics
                {
                    ProgressIndication = "Almost done",
                    HoursSpent = 24
                }
            }
        };
    }
}
