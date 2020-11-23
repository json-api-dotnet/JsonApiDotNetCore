using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OperationsExample
{
    public class TestStartup : Startup
    {
        public TestStartup(IConfiguration configuration)
            : base(configuration)
        {
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISystemClock, TickingSystemClock>();

            base.ConfigureServices(services);
        }

        /// <summary>
        /// Advances the clock one second each time the current time is requested.
        /// </summary>
        private class TickingSystemClock : ISystemClock
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

            public TickingSystemClock()
                : this(new DateTimeOffset(new DateTime(2000, 1, 1)))
            {
            }

            public TickingSystemClock(DateTimeOffset utcNow)
            {
                _utcNow = utcNow;
            }
        }
    }
}
