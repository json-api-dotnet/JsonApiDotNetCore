using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using JsonApiDotNetCore.Services;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace JsonApiDotNetCoreExample
{
    /// <summary>
    /// This should be in JsonApiDotNetCoreExampleTests project but changes in .net core 3.0
    /// do no longer allow that. See https://github.com/aspnet/AspNetCore/issues/15373.
    /// </summary>
    public class MetaStartup : Startup
    {
        public MetaStartup(IConfiguration configuration) : base(configuration)
        {
        }

        public new void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IRequestMeta, MetaService>();
            base.ConfigureServices(services);
        }
    }

    public class MetaService : IRequestMeta
    {
        public Dictionary<string, object> GetMeta()
        {
            return new Dictionary<string, object> {
                { "request-meta", "request-meta-value" }
            };
        }
    }
}
