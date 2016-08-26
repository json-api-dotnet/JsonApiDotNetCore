using JsonApiDotNetCore.Abstractions;
using JsonApiDotNetCore.JsonApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JsonApiDotNetCore.Services
{
  public class JsonApiSerializer
  {
    private readonly JsonApiContext _context;

    public JsonApiSerializer(JsonApiContext jsonApiContext)
    {
      _context = jsonApiContext;
    }

    public string ToJsonApiDocument(object resultValue)
    {
      var documentBuilder = new DocumentBuilder(_context, resultValue);
      var response = new JsonApiDocument
      {
        Links = documentBuilder.GetJsonApiDocumentLinks(),
        Data = documentBuilder.GetJsonApiDocumentData()
      };

      return JsonConvert.SerializeObject(response, new JsonSerializerSettings
      {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
      });
    }
  }
}
