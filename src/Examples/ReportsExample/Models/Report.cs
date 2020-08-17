using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;

namespace ReportsExample.Models
{
    public sealed class Report : Identifiable
    {
        [Attr]
        public string Title { get; set; }

        [Attr]
        public ReportStatistics Statistics { get; set; }
    }
}
