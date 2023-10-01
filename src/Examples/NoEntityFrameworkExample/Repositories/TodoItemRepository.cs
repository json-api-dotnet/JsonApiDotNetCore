using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using NoEntityFrameworkExample.Data;
using NoEntityFrameworkExample.Models;

namespace NoEntityFrameworkExample.Repositories;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TodoItemRepository : InMemoryResourceRepository<TodoItem, long>
{
    public TodoItemRepository(IResourceGraph resourceGraph, IQueryableBuilder queryableBuilder)
        : base(resourceGraph, queryableBuilder)
    {
    }

    protected override IEnumerable<TodoItem> GetDataSource()
    {
        return Database.TodoItems;
    }
}
