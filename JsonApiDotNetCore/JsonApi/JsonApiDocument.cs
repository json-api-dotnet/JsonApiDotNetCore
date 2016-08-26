using System.Collections.Generic;

namespace JsonApiDotNetCore.JsonApi
{
  public class JsonApiDocument
  {
    public Dictionary<string, string> Links { get; set; }
    // Data could be List<JsonApiDatum> or JsonApiDatum
    public object Data { get; set; }
  }
}
