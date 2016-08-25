using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using JsonApiDotNetCore.JsonApi;
using JsonApiDotNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Configuration
{
  public class JsonApiModelConfiguration : IJsonApiModelConfiguration
  {
    public string Namespace;
    public IMapper ResourceMapper;
    public Type ContextType { get; set; }
    public List<RouteDefinition> Routes = new List<RouteDefinition>();
    public Dictionary<Type, Type>  ResourceMapDefinitions = new Dictionary<Type, Type>();

    public void SetDefaultNamespace(string ns)
    {
      Namespace = ns;
    }

    public void DefineResourceMapping(Action<Dictionary<Type,Type>> mapping)
    {
      mapping.Invoke(ResourceMapDefinitions);

      var mapConfiguration = new MapperConfiguration(cfg =>
      {
        foreach (var definition in ResourceMapDefinitions)
        {
          cfg.CreateMap(definition.Key, definition.Value);
        }
      });

      ResourceMapper = mapConfiguration.CreateMapper();
    }

    public void UseContext<T>()
    {
      // TODO: assert the context is of type DbContext
      ContextType = typeof(T);
      LoadModelRoutesFromContext();
    }

    private void LoadModelRoutesFromContext()
    {
      // Assumption: all DbSet<> types should be included in the route list
      var properties = ContextType.GetProperties().ToList();

      properties.ForEach(property =>
      {
        if (property.PropertyType.GetTypeInfo().IsGenericType &&
          property.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
        {

          var modelType = property.PropertyType.GetGenericArguments()[0];

          var route = new RouteDefinition
          {
            ModelType = modelType,
            PathString = RouteBuilder.BuildRoute(Namespace, property.Name),
            ContextPropertyName = property.Name
          };

          Routes.Add(route);
        }
      });
    }

  }
}
