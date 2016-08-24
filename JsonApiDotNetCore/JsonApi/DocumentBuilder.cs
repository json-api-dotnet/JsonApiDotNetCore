using System.Collections.Generic;

namespace JsonApiDotNetCore.JsonApi
{
  public static class DocumentBuilder
  {
    public static Dictionary<string, string> BuildSelfLink(string protocol, string host, string nameSpace, string resourceCollectionName, string resourceId)
    {
      var id = resourceId != null ? $"/{resourceId}" : string.Empty;
      return new Dictionary<string, string>
      {
        {
          "self", $"{protocol}://{host}/{nameSpace}/{resourceCollectionName}{id}"
        }
      };
    }

    public static Dictionary<string, string> BuildRelationshipLinks(string protocol, string host, string nameSpace, string resourceCollectionName, string resourceId, string relationshipName)
    {
      return new Dictionary<string, string>
      {
        {"self", $"{protocol}://{host}/{nameSpace}/{resourceCollectionName}/{resourceId}/relationships/{relationshipName}"},
        {"related", $"{protocol}://{host}/{nameSpace}/{resourceCollectionName}/{resourceId}/{relationshipName}"}
      };
    }
  }
}
