using System;
using System.Text;
using System.Threading.Tasks;
using JsonApiDotNetCore.Abstractions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Routing
{
    public class Router : IRouter
    {
        private readonly JsonApiModelConfiguration _jsonApiModelConfiguration;
        private IServiceProvider _serviceProvider;
        private IRouteBuilder _routeBuilder;
        private IControllerBuilder _controllerBuilder;

        public Router(JsonApiModelConfiguration configuration, IRouteBuilder routeBuilder, IControllerBuilder controllerBuilder)
        {
            _jsonApiModelConfiguration = configuration;
            _routeBuilder = routeBuilder;
            _controllerBuilder = controllerBuilder;
        }

        public async Task<bool> HandleJsonApiRouteAsync(HttpContext context, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            var route = _routeBuilder.BuildFromRequest(context.Request);
            if (route == null) return false;

            var jsonApiContext = InitializeContext(context, route);
            await CallController(jsonApiContext);

            return true;
        }

        private JsonApiContext InitializeContext(HttpContext context, Route route)
        {
            var dbContext = _serviceProvider.GetService(_jsonApiModelConfiguration.ContextType);
            Console.WriteLine("InitializingContext");
            return new JsonApiContext(context, route, dbContext, _jsonApiModelConfiguration);
        }

        private async Task CallController(JsonApiContext jsonApiContext)
        {
            var controller = _controllerBuilder.BuildController(jsonApiContext);

            var result = ActivateControllerMethod(controller, jsonApiContext);

            result.Value = SerializeResult(result.Value, jsonApiContext);

            await SendResponse(jsonApiContext.HttpContext, result);
        }

        private ObjectResult ActivateControllerMethod(IJsonApiController controller, JsonApiContext jsonApiContext)
        {
            var route = jsonApiContext.Route;
            switch (route.RequestMethod)
            {
                case "GET":
                    return string.IsNullOrEmpty(route.ResourceId) ? controller.Get() : controller.Get(route.ResourceId);
                case "POST":
                    return controller.Post(new JsonApiDeserializer(jsonApiContext).GetEntityFromRequest());
                case "PATCH":
                    return controller.Patch(route.ResourceId, new JsonApiDeserializer(jsonApiContext).GetEntityPatch());
                case "DELETE":
                    return controller.Delete(route.ResourceId);
                default:
                    throw new ArgumentException("Request method not supported", nameof(route));
            }
        }

        private object SerializeResult(object result, JsonApiContext jsonApiContext)
        {
            return result == null ? null : new JsonApiSerializer(jsonApiContext).ToJsonApiDocument(result);
        }

        private async Task SendResponse(HttpContext context, ObjectResult result)
        {
            context.Response.StatusCode = result.StatusCode ?? 500;
            context.Response.ContentType = "application/vnd.api+json";
            await context.Response.WriteAsync(result.Value == null ? "" : result.Value.ToString(), Encoding.UTF8);
        }
    }
}
