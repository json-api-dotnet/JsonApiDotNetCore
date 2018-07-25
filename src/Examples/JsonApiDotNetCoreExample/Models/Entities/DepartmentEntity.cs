using JsonApiDotNetCore.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JsonApiDotNetCoreExample.Models.Entities
{
    [Table("Department")]
    public class DepartmentEntity : Identifiable
    {
        [Required]
        [StringLength(255, MinimumLength = 3)]
        public string Name { get; set; }

        public List<CourseEntity> Courses { get; set; }
    }
}
