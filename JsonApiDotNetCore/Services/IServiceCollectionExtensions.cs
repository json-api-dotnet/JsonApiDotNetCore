using System;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Services
{
    public static class IServiceCollectionExtensions
    {
        public static void AddJsonApiDotNetCore(this IServiceCollection services, Action<IJsonApiModelConfiguration> configurationAction)
        {
            var config = new JsonApiModelConfiguration();
            configurationAction.Invoke(config);
            services.AddSingleton(_ => new JsonApiService(config));
        }
    }
}
