using System;
using JsonApiDotNetCore.Abstractions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Extensions;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Routing
{
  public class RouteBuilder
  {
    private RouteDefinition _baseRouteDefinition;
    private string _baseResourceId;
    private readonly HttpRequest _request;
    private readonly JsonApiModelConfiguration _configuration;

    public RouteBuilder(HttpRequest request, JsonApiModelConfiguration configuration)
    {
      _request = request;
      _configuration = configuration;
    }

    public Route BuildFromRequest()
    {
      var remainingPathString = SetBaseRouteDefinition();

      if (PathStringIsEmpty(remainingPathString))
      { // {baseResource}
        return new Route(_baseRouteDefinition.ModelType, _request.Method, null, _baseRouteDefinition);
      }

      remainingPathString = SetBaseResourceId(remainingPathString);
      if (PathStringIsEmpty(remainingPathString))
      { // {baseResource}/{baseResourceId}
        return new Route(_baseRouteDefinition.ModelType, _request.Method, _baseResourceId, _baseRouteDefinition);
      }

      // { baseResource}/{ baseResourceId}/{relatedResourceName}
      var relatedResource = remainingPathString.ExtractFirstSegment(out remainingPathString);

      if (relatedResource == "relationships") // TODO: need to verify whether or not a relationship object self == related
      { // {baseResource}/{baseResourceId}/relationships/{relatedResourceName}
        relatedResource = remainingPathString.ExtractFirstSegment(out remainingPathString);
      }

      var relationshipType = GetTypeOfRelatedResource(relatedResource);
      return new RelationalRoute(_baseRouteDefinition.ModelType, _request.Method, _baseResourceId, _baseRouteDefinition, relationshipType, relatedResource);
    }

    private bool PathStringIsEmpty(PathString pathString)
    {
      return pathString.HasValue ? string.IsNullOrEmpty(pathString.ToString().TrimStart('/')) : true;
    }

    private PathString SetBaseRouteDefinition()
    {
      foreach (var rte in _configuration.Routes)
      {
        PathString remainingPathString;
        if (_request.Path.StartsWithSegments(new PathString(rte.PathString), StringComparison.OrdinalIgnoreCase, out remainingPathString))
        {
          _baseRouteDefinition = rte;
          return remainingPathString;
        }
      }
      throw new Exception("Route is not defined.");
    }

    private PathString SetBaseResourceId(PathString remainPathString)
    {
      _baseResourceId = remainPathString.ExtractFirstSegment(out remainPathString);
      return remainPathString;
    }

    private Type GetTypeOfRelatedResource(string relationshipName)
    {
      return ModelAccessor.GetTypeFromModelRelationshipName(_baseRouteDefinition.ModelType, relationshipName);
    }


    // TODO: Why is this here?
    public static string BuildRoute(string nameSpace, string resourceCollectionName)
    {
      return $"/{nameSpace}/{resourceCollectionName}";
    }
  }
}
