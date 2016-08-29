using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using JsonApiDotNetCore.Abstractions;
using JsonApiDotNetCore.Controllers;
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
    public Dictionary<Type, Type> ControllerOverrides = new Dictionary<Type, Type>();

    public void SetDefaultNamespace(string ns)
    {
      Namespace = ns;
    }

    // TODO: change to AddResourceMapping(Type, Type)
    public void AddResourceMapping(Type modelType, Type resourceType)
    {
      if (!resourceType.GetInterfaces().Contains(typeof(IJsonApiResource)))
        throw new ArgumentException("Specified type does not implement IJsonApiResource", nameof(resourceType));

      ResourceMapDefinitions.Add(modelType, resourceType);
    }

    public void UseController(Type modelType, Type controllerType)
    {
      if(!controllerType.GetInterfaces().Contains(typeof(IJsonApiController)))
        throw new ArgumentException("Specified type does not implement IJsonApiController", nameof(controllerType));

      ControllerOverrides[modelType] = controllerType;
    }

    public void UseContext<T>()
    {
      ContextType = typeof(T);

      if (!typeof(DbContext).IsAssignableFrom(ContextType))
        throw new ArgumentException("Context Type must derive from DbContext", nameof(T));
    }
  }
}
