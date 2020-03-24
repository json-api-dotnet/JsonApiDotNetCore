using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class Passport : Identifiable
    {
        public int? SocialSecurityNumber { get; set; }
        public bool IsLocked { get; set; }

        [HasOne]
        public Person Person { get; set; }
    }
}
