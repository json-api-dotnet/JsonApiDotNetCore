using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class User : Identifiable
    {
        [Attr] public string Username { get; set; }
        [Attr] public string Password { get; set; }
    }

    public sealed class SuperUser : User
    {
        [Attr] public int SecurityLevel { get; set; }
    }
}
