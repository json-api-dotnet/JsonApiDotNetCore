using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using JsonApiDotNetCore.Abstractions;
using JsonApiDotNetCore.Attributes;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.JsonApi;
using JsonApiDotNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Services
{
  public class JsonApiService
  {
    private readonly JsonApiModelConfiguration _jsonApiModelConfiguration;
    private IServiceProvider _serviceProvider;

    public JsonApiService(JsonApiModelConfiguration configuration)
    {
      _jsonApiModelConfiguration = configuration;
    }

    public bool HandleJsonApiRoute(HttpContext context, IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider;

      var route = _jsonApiModelConfiguration.GetRouteForRequest(context.Request);
      if (route == null) return false;

      CallControllerMethod(route, context);

      return true;
    }

    private void CallControllerMethod(Route route, HttpContext context)
    {
      var dbContext = _serviceProvider.GetService(_jsonApiModelConfiguration.ContextType);
      var jsonApiContext = new JsonApiContext(context, route, dbContext);
      var controller = new JsonApiController(context, jsonApiContext);

      switch (route.RequestMethod)
      {
        case "GET":
          if (string.IsNullOrEmpty(route.ResourceId))
          {
            var result = controller.Get();
            result.Value = SerializeResponse(jsonApiContext, result.Value);
            SendResponse(context, result);
          }
          else
          {
            var result = controller.Get(route.ResourceId);
            result.Value = SerializeResponse(jsonApiContext, result.Value);
            SendResponse(context, result);
          }
          break;
        case "POST":
          controller.Post(null);
          break;
        case "PUT":
          controller.Put(route.ResourceId, null);
          break;
        case "DELETE":
          controller.Delete(route.ResourceId);
          break;
        default:
          throw new ArgumentException("Request method not supported", nameof(route));
      }
    }

    private string SerializeResponse(JsonApiContext jsonApiContext, object resultValue)
    {
      var response = new JsonApiDocument
      {
        Links = GetJsonApiDocumentLinks(jsonApiContext),
        Data = GetJsonApiDocumentData(jsonApiContext, resultValue)
      };

      return JsonConvert.SerializeObject(response, new JsonSerializerSettings
      {
          ContractResolver = new CamelCasePropertyNamesContractResolver()
      });
    }

    private object GetJsonApiDocumentData(JsonApiContext context, object result)
    {
      var enumerableResult = result as IEnumerable;

      if (enumerableResult == null) return ResourceToJsonApiDatum(context, EntityToJsonApiResource(result));

      var data = new List<JsonApiDatum>();
      foreach (var resource in enumerableResult)
      {
        data.Add(ResourceToJsonApiDatum(context, EntityToJsonApiResource(resource)));
      }
      return data;
    }

    private IJsonApiResource EntityToJsonApiResource(object entity)
    {
      var resource = entity as IJsonApiResource;
      if (resource != null) return resource;

      var attributes = TypeDescriptor.GetAttributes(entity);
      var type = ((JsonApiResourceAttribute)attributes[typeof(JsonApiResourceAttribute)]).JsonApiResourceType;
      return (IJsonApiResource)_jsonApiModelConfiguration.ResourceMaps.Map(entity, entity.GetType(), type);
    }

    private JsonApiDatum ResourceToJsonApiDatum(JsonApiContext context, IJsonApiResource resource)
    {
      return new JsonApiDatum
      {
        Type = context.Route.RouteDefinition.ContextPropertyName.ToCamelCase(),
        Id = resource.Id,
        Attributes = GetAttributesFromResource(resource),
        Links = GetJsonApiDatumLinks(context, resource),
        Relationships = BuildRelationshipsObject(context, resource)
      };
    }

    private static Dictionary<string, object> GetAttributesFromResource(IJsonApiResource resource)
    {
      return resource.GetType().GetProperties()
        .Where(propertyInfo => propertyInfo.GetMethod.IsVirtual == false)
        .ToDictionary(
          propertyInfo => propertyInfo.Name, propertyInfo => propertyInfo.GetValue(resource)
        );
    }

    private static void SendResponse(HttpContext context, ObjectResult result)
    {
      context.Response.StatusCode = result.StatusCode ?? 500;
      context.Response.ContentType = "application/vnd.api+json";
      context.Response.WriteAsync(result.Value.ToString());
      context.Response.Body.Flush();
    }

    private Dictionary<string, string> GetJsonApiDocumentLinks(JsonApiContext jsonApiContext)
    {
      var request = jsonApiContext.HttpContext.Request;
      var route = jsonApiContext.Route;

      return DocumentBuilder.BuildSelfLink(request.Scheme, request.Host.ToString(), _jsonApiModelConfiguration.Namespace,
        route.RouteDefinition.ContextPropertyName.ToCamelCase(), route.ResourceId);
    }

    private Dictionary<string, string> GetJsonApiDatumLinks(JsonApiContext jsonApiContext, IJsonApiResource resource)
    {
      return DocumentBuilder.BuildSelfLink(jsonApiContext.HttpContext.Request.Scheme,
        jsonApiContext.HttpContext.Request.Host.ToString(), _jsonApiModelConfiguration.Namespace,
        jsonApiContext.Route.RouteDefinition.ContextPropertyName.ToCamelCase(), resource.Id);
    }

    private Dictionary<string, object> BuildRelationshipsObject(JsonApiContext jsonApiContext, IJsonApiResource resource)
    {
      var relationships = new Dictionary<string, object>();
      jsonApiContext.Route.Model.GetProperties().Where(propertyInfo => propertyInfo.GetMethod.IsVirtual).ToList().ForEach(
        virtualProperty =>
        {
          relationships.Add(virtualProperty.Name, GetRelationshipLinks(jsonApiContext, resource, virtualProperty.Name.ToCamelCase()));
        });
      return relationships;
    }

    private Dictionary<string, string> GetRelationshipLinks(JsonApiContext jsonApiContext, IJsonApiResource resource, string relationshipName)
    {
      return DocumentBuilder.BuildRelationshipLinks(jsonApiContext.HttpContext.Request.Scheme,
        jsonApiContext.HttpContext.Request.Host.ToString(), _jsonApiModelConfiguration.Namespace,
        jsonApiContext.Route.RouteDefinition.ContextPropertyName.ToCamelCase(), resource.Id, relationshipName);
    }
  }
}
