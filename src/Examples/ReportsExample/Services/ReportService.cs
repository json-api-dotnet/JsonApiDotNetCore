using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

public class ReportService : IGetAllService<Report>
{
    private ILogger<ReportService> _logger;

    public ReportService(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ReportService>();
    }

    public Task<IEnumerable<Report>> GetAsync()
    {
        _logger.LogError("GetAsync");

        var task = new Task<IEnumerable<Report>>(() => Get());
        
        task.RunSynchronously(TaskScheduler.Default);

        return task;
    }

    private IEnumerable<Report> Get()
    {
        return new List<Report> {
            new Report {
                Title = "My Report",
                ComplexType = new ComplexType {
                    CompoundPropertyName = "value"
                }
            }
        };
    }
}