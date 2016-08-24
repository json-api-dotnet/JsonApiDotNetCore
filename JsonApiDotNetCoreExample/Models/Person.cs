using System.Collections.Generic;
using JsonApiDotNetCore.Attributes;
using JsonApiDotNetCoreExample.Resources;

namespace JsonApiDotNetCoreExample.Models
{
  [JsonApiResource(typeof(PersonResource))]
  public class Person
  {
    public int Id { get; set; }
    public string Name { get; set; }

    public virtual List<TodoItem> TodoItems { get; set; }
  }
}
