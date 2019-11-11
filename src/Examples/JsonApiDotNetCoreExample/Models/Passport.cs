using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class Passport : Identifiable
    {
        public virtual int? SocialSecurityNumber { get; set; }
        public virtual bool IsLocked { get; set; }

        [HasOne]
        public virtual Person Person { get; set; }
    }
}