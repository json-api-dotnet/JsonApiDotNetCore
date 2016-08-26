using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Abstractions;
using JsonApiDotNetCore.Attributes;
using JsonApiDotNetCore.Extensions;

namespace JsonApiDotNetCore.JsonApi
{
  public  class DocumentBuilder
  {
    private readonly JsonApiContext _context;
    private readonly object _resource;

    public DocumentBuilder(JsonApiContext context, object resource)
    {
      _context = context;
      _resource = resource;
    }

    public Dictionary<string, string> GetJsonApiDocumentLinks()
    {
      var request = _context.HttpContext.Request;

      return new Dictionary<string, string> {
        {
          "self", $"{request.Scheme}://{request.Host}{request.Path}"
        }
      };
    }

    public object GetJsonApiDocumentData()
    {
      var enumerableResult = _resource as IEnumerable;

      if (enumerableResult == null) return ResourceToJsonApiDatum(EntityToJsonApiResource(_resource));

      var data = new List<JsonApiDatum>();
      foreach (var resource in enumerableResult)
      {
        data.Add(ResourceToJsonApiDatum(EntityToJsonApiResource(resource)));
      }
      return data;
    }

    private IJsonApiResource EntityToJsonApiResource(object entity)
    {
      var resource = entity as IJsonApiResource;
      if (resource != null) return resource;

      var attributes = TypeDescriptor.GetAttributes(entity);
      var type = ((JsonApiResourceAttribute)attributes[typeof(JsonApiResourceAttribute)]).JsonApiResourceType;
      return (IJsonApiResource)_context.Configuration.ResourceMapper.Map(entity, entity.GetType(), type);
    }

    private JsonApiDatum ResourceToJsonApiDatum(IJsonApiResource resource)
    {
      return new JsonApiDatum
      {
        Type = _context.GetEntityName(),
        Id = resource.Id,
        Attributes = GetAttributesFromResource(resource),
        Links = GetJsonApiDatumLinks(resource),
        Relationships = BuildRelationshipsObject(resource)
      };
    }

    private Dictionary<string, object> BuildRelationshipsObject(IJsonApiResource resource)
    {
      var relationships = new Dictionary<string, object>();
      _context.GetEntityType().GetProperties().Where(propertyInfo => propertyInfo.GetMethod.IsVirtual).ToList().ForEach(
        virtualProperty =>
        {
          relationships.Add(virtualProperty.Name, GetRelationshipLinks(resource, virtualProperty.Name.ToCamelCase()));
        });
      return relationships;
    }

    private Dictionary<string, object> GetAttributesFromResource(IJsonApiResource resource)
    {
      return resource.GetType().GetProperties()
        .Where(propertyInfo => propertyInfo.GetMethod.IsVirtual == false)
        .ToDictionary(
          propertyInfo => propertyInfo.Name, propertyInfo => propertyInfo.GetValue(resource)
        );
    }

    private Dictionary<string, string> GetJsonApiDatumLinks(IJsonApiResource resource)
    {
      return BuildSelfLink(_context.HttpContext.Request.Scheme,
        _context.HttpContext.Request.Host.ToString(), _context.Configuration.Namespace,
        _context.GetEntityName(), resource.Id);
    }

    private Dictionary<string, string> BuildSelfLink(string protocol, string host, string nameSpace, string resourceCollectionName, string resourceId)
    {
      var id = resourceId != null ? $"/{resourceId}" : string.Empty;
      return new Dictionary<string, string>
      {
        {
          "self", $"{protocol}://{host}/{nameSpace}/{resourceCollectionName}{id}"
        }
      };
    }

    private Dictionary<string, string> GetRelationshipLinks(IJsonApiResource resource, string relationshipName)
    {
      return BuildRelationshipLinks(_context.HttpContext.Request.Scheme,
        _context.HttpContext.Request.Host.ToString(), _context.Configuration.Namespace,
       _context.GetEntityName(), resource.Id, relationshipName);
    }

    private Dictionary<string, string> BuildRelationshipLinks(string protocol, string host, string nameSpace, string resourceCollectionName, string resourceId, string relationshipName)
    {
      return new Dictionary<string, string>
      {
        {"self", $"{protocol}://{host}/{nameSpace}/{resourceCollectionName}/{resourceId}/relationships/{relationshipName}"},
        {"related", $"{protocol}://{host}/{nameSpace}/{resourceCollectionName}/{resourceId}/{relationshipName}"}
      };
    }
  }
}
