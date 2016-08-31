using System;
using JsonApiDotNetCore.Abstractions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Routing.Query;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Routing
{
  public class RouteBuilder : IRouteBuilder
  {
    private RouteDefinition _baseRouteDefinition;
    private string _baseResourceId;
    private readonly JsonApiModelConfiguration _configuration;

    public RouteBuilder(JsonApiModelConfiguration configuration)
    {
      _configuration = configuration;
    }

    public Route BuildFromRequest(HttpRequest request)
    {
      var remainingPathString = SetBaseRouteDefinition(request.Path);

      var querySet = new QuerySet(request.Query);

      if (PathStringIsEmpty(remainingPathString))
      { // {baseResource}
        return new Route(_baseRouteDefinition.ModelType, request.Method, null, _baseRouteDefinition, querySet);
      }

      remainingPathString = SetBaseResourceId(remainingPathString);
      if (PathStringIsEmpty(remainingPathString))
      { // {baseResource}/{baseResourceId}
        return new Route(_baseRouteDefinition.ModelType, request.Method, _baseResourceId, _baseRouteDefinition, querySet);
      }

      // { baseResource}/{ baseResourceId}/{relatedResourceName}
      var relatedResource = remainingPathString.ExtractFirstSegment(out remainingPathString);

      if (relatedResource == "relationships") // TODO: need to verify whether or not a relationship object self == related
      { // {baseResource}/{baseResourceId}/relationships/{relatedResourceName}
        relatedResource = remainingPathString.ExtractFirstSegment(out remainingPathString);
      }

      var relationshipType = GetTypeOfRelatedResource(relatedResource);
      return new RelationalRoute(_baseRouteDefinition.ModelType, request.Method, _baseResourceId, _baseRouteDefinition, querySet, relationshipType, relatedResource);
    }

    private bool PathStringIsEmpty(PathString pathString)
    {
      return pathString.HasValue ? string.IsNullOrEmpty(pathString.ToString().TrimStart('/')) : true;
    }

    private PathString SetBaseRouteDefinition(PathString path)
    {
      foreach (var rte in _configuration.Routes)
      {
        PathString remainingPathString;
        if (path.StartsWithSegments(new PathString(rte.PathString), StringComparison.OrdinalIgnoreCase, out remainingPathString))
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
