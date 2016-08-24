using System.Collections.Generic;

namespace JsonApiDotNetCore.JsonApi
{
  public class JsonApiDatum
  {
    public string Type { get; set; }
    public string Id { get; set; }
    public Dictionary<string, object> Attributes { get; set; }
    public Dictionary<string, object> Relationships { get; set; }
    public Dictionary<string, string> Links { get; set; }
  }
}
