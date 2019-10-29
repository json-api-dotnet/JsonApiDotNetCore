using JsonApiDotNetCore.Models;

namespace NoEntityFrameworkExample.Models
{
    public class Passport : Identifiable
    {
        public virtual int? SocialSecurityNumber { get; set; }
        public virtual bool IsLocked { get; set; } 

        [HasOne("person")]
        public virtual Person Person { get; set; }
    }
}