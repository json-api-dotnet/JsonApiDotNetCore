using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCoreExample.Data;
using Microsoft.AspNetCore.Authentication;

namespace JsonApiDotNetCoreExample.Models
{
    public class User : Identifiable
    {
        private readonly ISystemClock _systemClock;
        private string _password;

        [Attr] public string UserName { get; set; }

        [Attr(Capabilities = AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange)]
        public string Password
        {
            get => _password;
            set
            {
                if (value != _password)
                {
                    _password = value;
                    LastPasswordChange = _systemClock.UtcNow.LocalDateTime;
                }
            }
        }

        [Attr] public DateTime LastPasswordChange { get; set; }

        public User(AppDbContext appDbContext)
        {
            _systemClock = appDbContext.SystemClock;
        }
    }

    public sealed class SuperUser : User
    {
        [Attr] public int SecurityLevel { get; set; }

        public SuperUser(AppDbContext appDbContext) : base(appDbContext)
        {
        }
    }
}
