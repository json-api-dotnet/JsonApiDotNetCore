using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using Microsoft.EntityFrameworkCore.Metadata;
using NoEntityFrameworkExample.Data;
using NoEntityFrameworkExample.Models;

namespace NoEntityFrameworkExample.Repositories;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TodoItemRepository(IResourceGraph resourceGraph, IQueryableBuilder queryableBuilder, IReadOnlyModel entityModel)
    : InMemoryResourceRepository<TodoItem, long>(resourceGraph, queryableBuilder, entityModel)
{
    protected override IEnumerable<TodoItem> GetDataSource()
    {
        return Database.TodoItems;
    }
}
