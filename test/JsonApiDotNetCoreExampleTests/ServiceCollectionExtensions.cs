using JsonApiDotNetCoreExample;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCoreExampleTests
{
    public static class ServiceCollectionExtensions
    {
        public static void AddControllersFromExampleProject(this IServiceCollection services)
        {
            var part = new AssemblyPart(typeof(EmptyStartup).Assembly);
            services.AddMvcCore().ConfigureApplicationPartManager(apm => apm.ApplicationParts.Add(part));
        }
    }
}
