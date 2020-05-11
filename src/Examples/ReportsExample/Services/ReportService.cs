using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;
using ReportsExample.Models;

namespace ReportsExample.Services
{
    public class ReportService : IGetAllService<Report>
    {
        private readonly ILogger<ReportService> _logger;

        public ReportService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ReportService>();
        }

        public Task<IEnumerable<Report>> GetAsync()
        {
            _logger.LogInformation("GetAsync");

            IEnumerable<Report> reports = GetReports();

            return Task.FromResult(reports);
        }

        private IEnumerable<Report> GetReports()
        {
            return new List<Report>
            {
                new Report
                {
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
}
