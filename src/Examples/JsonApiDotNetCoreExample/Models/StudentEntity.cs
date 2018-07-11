using JsonApiDotNetCore.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JsonApiDotNetCoreExample.Models
{
    [Table("student")]
    public class StudentEntity : Identifiable
    {
        [Column("first-name")]
        [Required]
        public string FirstName { get; set; }

        [Column("last-name")]
        [Required]
        [StringLength(255, MinimumLength = 3)]
        public string LastName { get; set; }

        [Column("address")]
        public string Address { get; set; }
    }
}
