using JsonApiDotNetCore.Models;
using System.Collections.Generic;

namespace JsonApiDotNetCoreExample.Models.Resources
{
    public class DepartmentResource : Identifiable
    {
        [Attr("name")]
        public string Name { get; set; }

        [HasMany("courses")]
        public List<CourseResource> Courses { get; set; }
    }
}
