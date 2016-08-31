using System;
using JsonApiDotNetCore.Routing.Query;

namespace JsonApiDotNetCore.Routing
{
  public class RelationalRoute : Route
  {
    public RelationalRoute(Type baseModelType, string requestMethod, string resourceId, RouteDefinition baseRouteDefinition, QuerySet querySet, Type relationalType, string relationshipName)
      : base(baseModelType, requestMethod, resourceId, baseRouteDefinition, querySet)
    {
      RelationalType = relationalType;
      RelationshipName = relationshipName;
    }

    public Type RelationalType { get; set; }
    public string RelationshipName { get; set; }
  }
}
