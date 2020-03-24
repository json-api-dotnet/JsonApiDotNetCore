using JsonApiDotNetCore.Models;

namespace ReportsExample.Models
{
    public sealed class Report : Identifiable
    {
        [Attr]
        public string Title { get; set; }
    
        [Attr]
        public ComplexType ComplexType { get; set; }
    }

    public sealed class ComplexType
    {
        public string CompoundPropertyName { get; set; }
    }
}
