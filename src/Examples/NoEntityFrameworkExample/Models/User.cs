using System;
using JsonApiDotNetCore.Models;

namespace NoEntityFrameworkExample.Models
{
    public class User : Identifiable
    {
        [Attr] public string Username { get; set; }
        [Attr] public string Password { get; set; }
    }
}
