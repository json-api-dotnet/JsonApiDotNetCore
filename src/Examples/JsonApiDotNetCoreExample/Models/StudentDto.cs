using JsonApiDotNetCore.Models;
using System.ComponentModel.DataAnnotations;

namespace JsonApiDotNetCoreExample.Models
{
    public class StudentDto : Identifiable
    {
        [Attr("name")]
        [Required]
        public string Name { get; set; }

        [Attr("address")]
        public string Address { get; set; }
    }
}
