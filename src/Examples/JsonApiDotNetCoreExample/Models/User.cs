using System;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class User : Identifiable
    {
        private string _password;

        [Attr] public string Username { get; set; }

        [Attr]
        public string Password
        {
            get => _password;
            set
            {
                if (value != _password)
                {
                    _password = value;
                    LastPasswordChange = DateTime.Now;
                }
            }
        }

        [Attr] public DateTime LastPasswordChange { get; set; }
    }

    public sealed class SuperUser : User
    {
        [Attr] public int SecurityLevel { get; set; }
    }
}
