using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using JsonApiDotNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Configuration
{
  public class JsonApiModelConfiguration : IJsonApiModelConfiguration
  {
    public string Namespace;
    public IMapper ResourceMaps;
    public Type ContextType { get; set; }

    private readonly List<RouteDefinition> _routes = new List<RouteDefinition>();

    public void SetDefaultNamespace(string ns)
    {
      Namespace = ns;
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

          var route = new RouteDefinition
          {
            ModelType = modelType,
            PathString = RouteBuilder.BuildRoute(Namespace, property.Name),
            ContextPropertyName = property.Name
          };

          _routes.Add(route);
        }
      });
    }

    public Route GetRouteForRequest(HttpRequest request)
    {
      foreach (var rte in _routes)
      {
        PathString remainingPathString;
        if (request.Path.StartsWithSegments(new PathString(rte.PathString), StringComparison.OrdinalIgnoreCase, out remainingPathString))
        {
          return new Route(rte.ModelType, request.Method, remainingPathString, rte);
        }
      }
      return null;
    }
  }
}
