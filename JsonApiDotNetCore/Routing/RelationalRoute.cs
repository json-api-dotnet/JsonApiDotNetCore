using System;

namespace JsonApiDotNetCore.Routing
{
  public class RelationalRoute : Route
  {
    public RelationalRoute(Type baseModelType, string requestMethod, string resourceId, RouteDefinition baseRouteDefinition, Type relationalType, string relationshipName)
      : base(baseModelType, requestMethod, resourceId, baseRouteDefinition)
    {
      RelationalType = relationalType;
      RelationshipName = relationshipName;
    }

    public Type RelationalType { get; set; }
    public string RelationshipName { get; set; }
  }
}
