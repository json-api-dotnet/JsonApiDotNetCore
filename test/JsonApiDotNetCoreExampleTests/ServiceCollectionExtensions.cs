using JsonApiDotNetCoreExample.Startups;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCoreExampleTests
{
    internal static class ServiceCollectionExtensions
    {
        public static void AddControllersFromExampleProject(this IServiceCollection services)
        {
            var part = new AssemblyPart(typeof(EmptyStartup).Assembly);
            services.AddMvcCore().ConfigureApplicationPartManager(apm => apm.ApplicationParts.Add(part));
        }
    }
}
