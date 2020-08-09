using System;
using System.ComponentModel.DataAnnotations;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCoreExample.Models
{
    public class TeamPlayer
    {
        [Key]
        [Required]
        public Guid TeamId { get; set; }
        [HasOne]
        public Team Team { get; set; }

        [Key]
        [Required]
        public Guid PlayerId { get; set; }
        [HasOne]
        public Player Player { get; set; }
    }
}
