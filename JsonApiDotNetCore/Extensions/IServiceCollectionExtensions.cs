using System;
using AutoMapper;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Extensions
{
  public static class IServiceCollectionExtensions
  {
    public static void AddJsonApi(this IServiceCollection services, Action<IJsonApiModelConfiguration> configurationAction)
    {
      var config = new JsonApiModelConfiguration();
      configurationAction.Invoke(config);

      if (config.ResourceMapper == null)
      {
        config.ResourceMapper = new MapperConfiguration(cfg => {}).CreateMapper();
      }
      services.AddSingleton(_ => new Router(config));
    }
  }
}
