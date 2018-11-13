using JsonApiDotNetCore.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JsonApiDotNetCoreExample.Models.Entities;

namespace JsonApiDotNetCoreExample.Models.Resources
{
    public class CourseResource : Identifiable
    {
        [Attr("number")]
        [Required]
        public int Number { get; set; }

        [Attr("title")]
        [Required]
        public string Title { get; set; }

        [Attr("description")]
        public string Description { get; set; }

        [HasOne("department", withEntity: "Department")]
        public DepartmentResource Department { get; set; }
        public int? DepartmentId { get; set; }

        [HasMany("students")]
        public List<StudentResource> Students { get; set; }
    }
}
