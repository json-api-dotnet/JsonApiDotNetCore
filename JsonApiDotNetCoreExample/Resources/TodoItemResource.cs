using System.Collections.Generic;
using JsonApiDotNetCore.Abstractions;

namespace JsonApiDotNetCoreExample.Resources
{
  public class TodoItemResource : IJsonApiResource
  {
    public string Id { get; set; }
    public string Name { get; set; }
  }
}
