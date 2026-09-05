using JetBrains.Annotations;
using MongoDB.Bson;

namespace JsonApiDotNetCoreExample.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class TagTodoItem
{
    public ObjectId TagId { get; set; }
    public ObjectId TodoItemId { get; set; }

    public Tag Tag { get; set; } = null!;
    public TodoItem TodoItem { get; set; } = null!;
}
