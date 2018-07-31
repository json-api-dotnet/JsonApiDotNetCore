using JsonApiDotNetCore.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JsonApiDotNetCoreExample.Models.Resources
{
    public class StudentResource : Identifiable
    {
        [Attr("firstname")]
        [Required]
        public string FirstName { get; set; }

        [Attr("lastname")]
        [Required]
        public string LastName { get; set; }

        [Attr("address")]
        public string Address { get; set; }

        [HasMany("courses")]
        public List<CourseResource> Courses { get; set; }
    }
}
