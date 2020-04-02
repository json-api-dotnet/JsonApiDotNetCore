using System.ComponentModel.DataAnnotations;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class Tag : Identifiable
    {
        [Attr]
        [RegularExpression(@"^\W$")]
        public string Name { get; set; }
    }
}
