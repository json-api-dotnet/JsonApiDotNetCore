using JsonApiDotNetCore.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JsonApiDotNetCoreExample.Models.Entities
{
    [Table("Student")]
    public class StudentEntity : Identifiable
    {
        [Column("firstname")]
        [Required]
        public string FirstName { get; set; }

        [Column("lastname")]
        [Required]
        [StringLength(255, MinimumLength = 3)]
        public string LastName { get; set; }

        [Column("address")]
        public string Address { get; set; }

        public List<CourseStudentEntity> Courses { get; set; }
    }
}
