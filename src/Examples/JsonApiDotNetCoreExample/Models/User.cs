using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class User : Identifiable
    {
        [Attr("username")] public string Username { get; set; }
        [Attr("password")] public string Password { get; set; }
    }
}
