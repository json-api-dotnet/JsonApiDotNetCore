using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Abstractions;
using JsonApiDotNetCore.Data;

namespace JsonApiDotNetCore.Controllers
{
  public class ControllerBuilder
  {
    private readonly JsonApiContext _context;

    public ControllerBuilder(JsonApiContext context)
    {
      _context = context;
    }

    public IJsonApiController BuildController()
    {
      var overrideController = GetOverrideController();
      return overrideController ?? new JsonApiController(_context, new ResourceRepository(_context));
    }

    public IJsonApiController GetOverrideController()
    {
      Type controllerType;
      return _context.Configuration.ControllerOverrides.TryGetValue(_context.GetEntityType(), out controllerType) ?
        InstantiateController(controllerType) : null;
    }

    private IJsonApiController InstantiateController(Type controllerType)
    {
      var constructor = controllerType.GetConstructors()[0];
      var parameters = constructor.GetParameters();
      var services =
        parameters.Select(param => GetService(param.ParameterType)).ToArray();
      return (IJsonApiController) Activator.CreateInstance(controllerType, services);
    }

    private object GetService(Type serviceType)
    {
      if(serviceType == typeof(ResourceRepository))
          return new ResourceRepository(_context);
      if (serviceType == typeof(JsonApiContext))
          return _context;

      return _context.HttpContext.RequestServices.GetService(serviceType);
    }
  }
}
