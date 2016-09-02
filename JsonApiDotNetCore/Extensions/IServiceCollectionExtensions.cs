using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void AddJsonApi(this IServiceCollection services, Action<IJsonApiModelConfiguration> configurationAction)
        {
            var configBuilder = new JsonApiConfigurationBuilder(configurationAction);
            var config = configBuilder.Build();
            services.AddTransient(_ =>
            {
              return (IRouter)new Router(config, new RouteBuilder(config), new ControllerBuilder());
            });
        }
    }
}
