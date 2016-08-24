using System;
using Microsoft.AspNetCore.Http;
using JsonApiDotNetCore.Controllers;

namespace JsonApiDotNetCore.Services
{
  public class JsonApiService
  {
    private JsonApiModelConfiguration _jsonApiModelConfiguration;
    private IServiceProvider _serviceProvider;

    public JsonApiService(JsonApiModelConfiguration configuration)
    {
      _jsonApiModelConfiguration = configuration;
    }

    public bool HandleJsonApiRoute(HttpContext context, IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider;

      var route = context.Request.Path;
      var requestMethod = context.Request.Method;
      var controllerMethodIdentifier = _jsonApiModelConfiguration.GetControllerMethodIdentifierForRoute(route, requestMethod);
      if (controllerMethodIdentifier == null) return false;
      CallControllerMethod(controllerMethodIdentifier, context);
      return true;
    }

    private void CallControllerMethod(ControllerMethodIdentifier controllerMethodIdentifier, HttpContext context)
    {
      var dbContext = _serviceProvider.GetService(_jsonApiModelConfiguration.ContextType);
      var jsonApiContext = new JsonApiContext(controllerMethodIdentifier.Route, dbContext);
      var controller = new JsonApiController(context, jsonApiContext);
      var resourceId = controllerMethodIdentifier.GetResourceId();
      switch(controllerMethodIdentifier.RequestMethod)
      {
        case "GET":
          if(string.IsNullOrEmpty(resourceId))
          {
            var result = controller.Get();
            // TODO:
            // - convert the entity to a JSON API Document
            // - write the response
          }
          else
          {
            controller.Get(resourceId);
          }
          break;
        case "POST":
          controller.Post(null); // TODO: need the request body
          break;
        case "PUT":
          controller.Put(resourceId, null);
          break;
        case "DELETE":
          controller.Delete(resourceId);
          break;
      }
    }
  }
}
