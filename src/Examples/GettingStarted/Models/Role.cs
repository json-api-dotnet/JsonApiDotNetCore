using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace GettingStarted.Models
{
    public class Role : Identifiable
    {
        [Attr]
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [NotMapped]
        [HasManyThrough(nameof(UserRoles))]
        public ICollection<UserProfile> Users { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
