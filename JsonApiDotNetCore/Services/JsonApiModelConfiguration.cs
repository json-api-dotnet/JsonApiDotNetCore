using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using JsonApiDotNetCore.Controllers;

namespace JsonApiDotNetCore.Services
{
    public class JsonApiModelConfiguration : IJsonApiModelConfiguration
    {
        private string _namespace;
        private List<Route> _routes = new List<Route>();
        public Type ContextType { get; set; }

        public void SetDefaultNamespace(string ns)
        {
            _namespace = ns;
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

          properties.ForEach(property => {
            if(property.PropertyType.GetTypeInfo().IsGenericType &&
              property.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)) {

              var modelType = property.PropertyType.GetGenericArguments()[0];

              var route = new Route {
                ModelType = modelType,
                PathString = BuildRoute(modelType),
                ContextPropertyName = property.Name
              };

              _routes.Add(route);
            }
          });
        }

        public ControllerMethodIdentifier GetControllerMethodIdentifierForRoute(PathString route, string requestMethod)
        {
          PathString remainingPathString;

          foreach(Route rte in _routes)
          {
            if(route.StartsWithSegments(new PathString(rte.PathString), StringComparison.OrdinalIgnoreCase, out remainingPathString))
            {
              return new ControllerMethodIdentifier(rte.ModelType, requestMethod, remainingPathString, rte);
            }
          }
          return null;
        }

        private string BuildRoute(Type type)
        {
          return $"/{_namespace}/{GetModelRouteName(type)}";
        }

        private string GetModelRouteName(Type type)
        {
          var attributes = TypeDescriptor.GetAttributes(type);
          return GetPluralNameFromAttributes(attributes);
        }

        private static string GetPluralNameFromAttributes(AttributeCollection attributes)
        {
          return ((SerializationFormat)attributes[typeof(SerializationFormat)])?.PluralName;
        }
    }
}
