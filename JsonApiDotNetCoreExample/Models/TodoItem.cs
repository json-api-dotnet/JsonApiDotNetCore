using JsonApiDotNetCore.Attributes;
using JsonApiDotNetCoreExample.Resources;

namespace JsonApiDotNetCoreExample.Models
{
  [JsonApiResource(typeof(TodoItemResource))]
  public class TodoItem
  {
    public int Id { get; set; }
    public string Name { get; set; }
  }
}
