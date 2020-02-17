using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using JsonApiDotNetCore.Services;
using System.Collections.Generic;

namespace JsonApiDotNetCoreExample
{
    /// <summary>
    /// This should be in JsonApiDotNetCoreExampleTests project but changes in .net core 3.0
    /// do no longer allow that. See https://github.com/aspnet/AspNetCore/issues/15373.
    /// </summary>
    public sealed class MetaStartup : Startup
    {
        public MetaStartup(IWebHostEnvironment env) : base(env) { }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IRequestMeta, MetaService>();
            base.ConfigureServices(services);
        }
    }

    public sealed class MetaService : IRequestMeta
    {
        public Dictionary<string, object> GetMeta()
        {
            return new Dictionary<string, object> {
                { "request-meta", "request-meta-value" }
            };
        }
    }
}
