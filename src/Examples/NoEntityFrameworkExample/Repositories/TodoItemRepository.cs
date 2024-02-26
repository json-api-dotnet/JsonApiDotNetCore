using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using NoEntityFrameworkExample.Data;
using NoEntityFrameworkExample.Models;

namespace NoEntityFrameworkExample.Repositories;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TodoItemRepository(IResourceGraph resourceGraph, IQueryableBuilder queryableBuilder)
    : InMemoryResourceRepository<TodoItem, long>(resourceGraph, queryableBuilder)
{
    protected override IEnumerable<TodoItem> GetDataSource()
    {
        return Database.TodoItems;
    }
}
