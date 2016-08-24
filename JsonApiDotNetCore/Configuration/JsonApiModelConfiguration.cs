using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using AutoMapper;
using JsonApiDotNetCore.Abstractions;
using JsonApiDotNetCore.Attributes;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Configuration
{
  public class JsonApiModelConfiguration : IJsonApiModelConfiguration
  {
    private string _namespace;
    private readonly List<Route> _routes = new List<Route>();

    public IMapper ResourceMaps;

    public Type ContextType { get; set; }

    public void SetDefaultNamespace(string ns)
    {
      _namespace = ns;
    }

    public void DefineResourceMapping(MapperConfiguration mapperConfiguration)
    {
      ResourceMaps = mapperConfiguration.CreateMapper();
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

          var route = new Route
          {
            ModelType = modelType,
            PathString = RouteBuilder.BuildRoute(_namespace, property.Name),
            ContextPropertyName = property.Name
          };

          _routes.Add(route);
        }
      });
    }

    public ControllerMethodIdentifier GetControllerMethodIdentifierForRoute(PathString route, string requestMethod)
    {
      foreach (var rte in _routes)
      {
        PathString remainingPathString;
        if (route.StartsWithSegments(new PathString(rte.PathString), StringComparison.OrdinalIgnoreCase, out remainingPathString))
        {
          return new ControllerMethodIdentifier(rte.ModelType, requestMethod, remainingPathString, rte);
        }
      }
      return null;
    }


  }
}
