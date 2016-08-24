using JsonApiDotNetCore.Abstractions;
using JsonApiDotNetCore.Attributes;

namespace JsonApiDotNetCore.Models
{
  [SerializationFormat("todoItem","todoItems")]
  public class TodoItem : IJsonApiResource
  {
    public string Id { get; set; }
    public string Name { get; set; }
  }
}
