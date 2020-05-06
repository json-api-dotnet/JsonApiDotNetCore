using JsonApiDotNetCore.Configuration;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json.Serialization;

namespace JsonApiDotNetCoreExample
{
    /// <summary>
    /// This should be in JsonApiDotNetCoreExampleTests project but changes in .net core 3.0
    /// do no longer allow that. See https://github.com/aspnet/AspNetCore/issues/15373.
    /// </summary>
    public sealed class KebabCaseStartup : TestStartup
    {
        public KebabCaseStartup(IWebHostEnvironment env) : base(env)
        {
        }

        protected override void ConfigureJsonApiOptions(JsonApiOptions options)
        {
            base.ConfigureJsonApiOptions(options);

            ((DefaultContractResolver)options.SerializerSettings.ContractResolver).NamingStrategy = new KebabCaseNamingStrategy();
        }
    }
}
