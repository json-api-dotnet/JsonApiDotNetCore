namespace GettingStarted.Models
{
    public class UserRole
    {
        public int UserProfileId { get; set; }
        public UserProfile UserProfile { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}
