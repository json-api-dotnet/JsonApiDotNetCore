using JsonApiDotNetCore.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace JsonApiDotNetCoreExample.Startups
{
    /// <summary>
    /// This should be in JsonApiDotNetCoreExampleTests project but changes in .net core 3.0
    /// do no longer allow that. See https://github.com/aspnet/AspNetCore/issues/15373.
    /// </summary>
    public class NoNamespaceStartup : TestStartup
    {
        public NoNamespaceStartup(IWebHostEnvironment env) : base(env)
        {
        }

        protected override void ConfigureJsonApiOptions(JsonApiOptions options)
        {
            base.ConfigureJsonApiOptions(options);

            options.Namespace = null;
        }
    }
}
