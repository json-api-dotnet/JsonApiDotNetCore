using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JsonApiDotNetCore.Abstractions;
using JsonApiDotNetCore.Attributes;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.JsonApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using JsonApiDotNetCore.Routing;

namespace JsonApiDotNetCore.Services
{
  public class JsonApiSerializer
  {
    private readonly JsonApiContext _context;
    private readonly JsonApiModelConfiguration _jsonApiModelConfiguration;
    private string _entityName;
    private Type _entityType;

    public JsonApiSerializer(JsonApiContext jsonApiContext, JsonApiModelConfiguration configuration)
    {
      _context = jsonApiContext;
      _jsonApiModelConfiguration = configuration;
      _entityName = GetEntityName();
      _entityType = GetEntityType();
    }

    private string GetEntityName() {
      return (!(_context.Route is RelationalRoute) ? _context.Route.BaseRouteDefinition.ContextPropertyName
      : _jsonApiModelConfiguration.Routes.Single(r=> r.ModelType == ((RelationalRoute)_context.Route).RelationalType).ContextPropertyName).ToCamelCase();
    }

    private Type GetEntityType() {
      return !(_context.Route is RelationalRoute) ? _context.Route.BaseRouteDefinition.ModelType
      : ((RelationalRoute)_context.Route).RelationalType;
    }

    public string ToJsonApiDocument(object resultValue)
    {
      var response = new JsonApiDocument
      {
        Links = GetJsonApiDocumentLinks(_context),
        Data = GetJsonApiDocumentData(_context, resultValue)
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
      return (IJsonApiResource)_jsonApiModelConfiguration.ResourceMapper.Map(entity, entity.GetType(), type);
    }

    private JsonApiDatum ResourceToJsonApiDatum(JsonApiContext context, IJsonApiResource resource)
    {
      return new JsonApiDatum
      {
        Type = _entityName,
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

    private Dictionary<string, string> GetJsonApiDocumentLinks(JsonApiContext jsonApiContext)
    {
      var request = jsonApiContext.HttpContext.Request;
      var route = jsonApiContext.Route;

      return new Dictionary<string, string> {
        {
          "self", $"{request.Scheme}://{request.Host}{request.Path}"
        }
      };
    }

    private Dictionary<string, string> GetJsonApiDatumLinks(JsonApiContext jsonApiContext, IJsonApiResource resource)
    {
      return DocumentBuilder.BuildSelfLink(jsonApiContext.HttpContext.Request.Scheme,
        jsonApiContext.HttpContext.Request.Host.ToString(), _jsonApiModelConfiguration.Namespace,
        _entityName, resource.Id);
    }

    private Dictionary<string, object> BuildRelationshipsObject(JsonApiContext jsonApiContext, IJsonApiResource resource)
    {
      var relationships = new Dictionary<string, object>();
      _entityType.GetProperties().Where(propertyInfo => propertyInfo.GetMethod.IsVirtual).ToList().ForEach(
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
       _entityName, resource.Id, relationshipName);
    }
  }
}
