using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class Passport : Identifiable
    {
        public virtual int? SocialSecurityNumber { get; set; }
        [HasOne("person")]
        public virtual Person Person { get; set; }
    }
}