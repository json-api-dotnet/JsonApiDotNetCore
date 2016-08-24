using JsonApiDotNetCore.Attributes;

namespace JsonApiDotNetCore.Models
{
  [SerializationFormat("todoItem","todoItems")]
  public class TodoItem
  {
    public int Id { get; set; }
    public string Name { get; set; }
  }
}
