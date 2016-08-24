using JsonApiDotNetCore.Abstractions;

namespace JsonApiDotNetCoreExample.Resources
{
  public class PersonResource : IJsonApiResource
  {
    public string Id { get; set; }
    public string Name { get; set; }
  }
}
