using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace GettingStarted.Models
{
    public class UserProfile : Identifiable
    {
        [Attr]
        public UserProfileStatus Status { get; set; } = UserProfileStatus.Setup;

        [Attr]
        public string SubjectId { get; set; }

        [Attr]
        [Required, MaxLength(100)]
        public string FirstName { get; set; }

        [Attr]
        [Required, MaxLength(100)]
        public string LastName { get; set; }

        [Attr]
        [Required, MaxLength(100), EmailAddress]
        public string Email { get; set; }

        [HasOne]
        public Org Org { get; set; }

        [NotMapped]
        [HasManyThrough(nameof(UserRoles))]
        public ICollection<Role> Roles { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }

        [Attr]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [HasOne]
        public UserProfile CreatedBy { get; set; }
    }
}
