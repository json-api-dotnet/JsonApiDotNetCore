using System;
using System.Text;
using JsonApiDotNetCore.Abstractions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Routing
{
  public class Router : IRouter
  {
    private readonly JsonApiModelConfiguration _jsonApiModelConfiguration;
    private IServiceProvider _serviceProvider;
    private JsonApiContext _jsonApiContext;
    private IRouteBuilder _routeBuilder;
    private IControllerBuilder _controllerBuilder;

    public Router(JsonApiModelConfiguration configuration, IRouteBuilder routeBuilder, IControllerBuilder controllerBuilder)
    {
      _jsonApiModelConfiguration = configuration;
      _routeBuilder = routeBuilder;
      _controllerBuilder = controllerBuilder;
    }

    public bool HandleJsonApiRoute(HttpContext context, IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider;

      var route = _routeBuilder.BuildFromRequest(context.Request);
      if (route == null) return false;

      InitializeContext(context, route);
      CallController();

      return true;
    }

    private void InitializeContext(HttpContext context, Route route)
    {
      var dbContext = _serviceProvider.GetService(_jsonApiModelConfiguration.ContextType);
      _jsonApiContext = new JsonApiContext(context, route, dbContext, _jsonApiModelConfiguration);
    }

    private void CallController()
    {
      var controller = _controllerBuilder.BuildController(_jsonApiContext);

      var result = ActivateControllerMethod(controller);

      result.Value = SerializeResult(result.Value);

      SendResponse(result);
    }

    private ObjectResult ActivateControllerMethod(IJsonApiController controller)
    {
      var route = _jsonApiContext.Route;
      switch (route.RequestMethod)
      {
        case "GET":
          return string.IsNullOrEmpty(route.ResourceId) ? controller.Get() : controller.Get(route.ResourceId);
        case "POST":
          return controller.Post(new JsonApiDeserializer(_jsonApiContext).GetEntityFromRequest());
        case "PATCH":
          return controller.Patch(route.ResourceId, new JsonApiDeserializer(_jsonApiContext).GetEntityPatch());
        case "DELETE":
          return controller.Delete(route.ResourceId);
        default:
          throw new ArgumentException("Request method not supported", nameof(route));
      }
    }

    private object SerializeResult(object result)
    {
      return result == null ? null : new JsonApiSerializer(_jsonApiContext).ToJsonApiDocument(result);
    }

    private void SendResponse(ObjectResult result)
    {
      var context = _jsonApiContext.HttpContext;
      context.Response.StatusCode = result.StatusCode ?? 500;
      context.Response.ContentType = "application/vnd.api+json";
      context.Response.WriteAsync(result.Value == null ? "" : result.Value.ToString(), Encoding.UTF8);
      context.Response.Body.Flush();
    }
  }
}
