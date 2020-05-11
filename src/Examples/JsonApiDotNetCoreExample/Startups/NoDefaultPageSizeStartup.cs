using Microsoft.AspNetCore.Hosting;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCoreExample
{
    /// <summary>
    /// This should be in JsonApiDotNetCoreExampleTests project but changes in .net core 3.0
    /// do no longer allow that. See https://github.com/aspnet/AspNetCore/issues/15373.
    /// </summary>
    public sealed class NoDefaultPageSizeStartup : TestStartup
    {
        public NoDefaultPageSizeStartup(IWebHostEnvironment env) : base(env)
        {
        }

        protected override void ConfigureJsonApiOptions(JsonApiOptions options)
        {
            base.ConfigureJsonApiOptions(options);

            options.DefaultPageSize = 0;
        }
    }
}
