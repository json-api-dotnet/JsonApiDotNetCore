using JsonApiDotNetCore.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JsonApiDotNetCoreExample.Models.Entities
{
    [Table("Course")]
    public class CourseEntity : Identifiable
    {
        [Column("number")]
        [Required]
        public int Number { get; set; }

        [Column("title")]
        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [Column("description")]
        [StringLength(4000)]
        public string Description { get; set; }

        public DepartmentEntity Department { get; set; }

        [Column("department_id")]
        public int? DepartmentId { get; set; }

        public List<CourseStudentEntity> Students { get; set; }
    }
}
