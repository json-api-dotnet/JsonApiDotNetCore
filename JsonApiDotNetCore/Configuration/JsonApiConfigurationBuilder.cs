using System;
using System.Reflection;
using JsonApiDotNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using JsonApiDotNetCore.Abstractions;
using JsonApiDotNetCore.Attributes;

namespace JsonApiDotNetCore.Configuration
{
  public class JsonApiConfigurationBuilder
  {
    private readonly Action<IJsonApiModelConfiguration> _configurationAction;
    private JsonApiModelConfiguration Config { get; set; }

    public JsonApiConfigurationBuilder(Action<IJsonApiModelConfiguration> configurationAction)
    {
      Config = new JsonApiModelConfiguration();
      _configurationAction = configurationAction;
    }

    public JsonApiModelConfiguration Build()
    {
      Config = new JsonApiModelConfiguration();
      _configurationAction.Invoke(Config);
      CheckIsValidConfiguration();
      LoadModelRoutesFromContext();
      SetupResourceMaps();
      return Config;
    }

    private void CheckIsValidConfiguration()
    {
      if (Config.ContextType == null)
        throw new NullReferenceException("DbContext is not specified");
    }

    private void LoadModelRoutesFromContext()
    {
      // Assumption: all DbSet<> types should be included in the route list
      var properties = Config.ContextType.GetProperties().ToList();

      properties.ForEach(property =>
      {
        if (property.PropertyType.GetTypeInfo().IsGenericType &&
          property.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
        {

          var modelType = property.PropertyType.GetGenericArguments()[0];

          var route = new RouteDefinition
          {
            ModelType = modelType,
            PathString = RouteBuilder.BuildRoute(Config.Namespace, property.Name),
            ContextPropertyName = property.Name
          };

          Config.Routes.Add(route);
        }
      });
    }

    private void SetupResourceMaps()
    {
      LoadDefaultResourceMaps();
      var mapConfiguration = new MapperConfiguration(cfg =>
      {
        foreach (var definition in Config.ResourceMapDefinitions)
        {
          var mappingExpression = cfg.CreateMap(definition.Key, definition.Value.Item1);
          definition.Value.Item2?.Invoke(mappingExpression);
        }
      });
      Config.ResourceMapper = mapConfiguration.CreateMapper();
    }

    private void LoadDefaultResourceMaps()
    {
      var resourceAttribute = typeof(JsonApiResourceAttribute);
      var modelTypes = Assembly.GetEntryAssembly().DefinedTypes.Where(t => t.GetCustomAttributes(resourceAttribute).Count() == 1);

      foreach (var modelType in modelTypes)
      {
        var resourceType = ((JsonApiResourceAttribute)modelType.GetCustomAttribute(resourceAttribute)).JsonApiResourceType;

        // do not overwrite custom definitions
        if(!Config.ResourceMapDefinitions.ContainsKey(modelType.UnderlyingSystemType))
        {
          Config.ResourceMapDefinitions.Add(modelType.UnderlyingSystemType, new Tuple<Type, Action<IMappingExpression>>(resourceType, null));
        }
      }
    }
  }
}
