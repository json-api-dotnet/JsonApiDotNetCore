using System;
using Microsoft.AspNetCore.Authentication;

namespace JsonApiDotNetCoreExampleTests
{
    internal class AlwaysChangingSystemClock : ISystemClock
    {
        private DateTimeOffset _utcNow;

        public DateTimeOffset UtcNow
        {
            get
            {
                var utcNow = _utcNow;
                _utcNow = _utcNow.AddSeconds(1);
                return utcNow;
            }
        }

        public AlwaysChangingSystemClock()
            : this(new DateTimeOffset(new DateTime(2000, 1, 1)))
        {
        }

        public AlwaysChangingSystemClock(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }
    }
}
