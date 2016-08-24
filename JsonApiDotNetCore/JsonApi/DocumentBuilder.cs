using System.Collections.Generic;

namespace JsonApiDotNetCore.JsonApi
{
  public static class DocumentBuilder
  {
    public static Dictionary<string, string> BuildSelfLink(string protocol, string host, string nameSpace, string modelRouteName, string resourceId)
    {
      var id = resourceId != null ? $"/{resourceId}" : string.Empty;
      return new Dictionary<string, string>
      {
        {
          "self", $"{protocol}://{host}/{nameSpace}/{modelRouteName}{id}"
        }
      };
    }
    public static Dictionary<string, string> BuildSelfLink(string nameSpace, string modelRouteName)
    {
      return new Dictionary<string, string>
      {
        {
          "self", $"/{nameSpace}/{modelRouteName}"
        }
      };
    }
    public static Dictionary<string, string> BuildRelationshipLinks(string protocol, string host, string nameSpace, string modelRouteName, string resourceId, string relationshipName)
    {
      return new Dictionary<string, string>
      {
        {"self", $"{protocol}://{host}/{nameSpace}/{modelRouteName}/{resourceId}/relationships/{relationshipName}"},
        {"related", $"{protocol}://{host}/{nameSpace}/{modelRouteName}/{resourceId}/{relationshipName}"}
      };
    }
  }
}
