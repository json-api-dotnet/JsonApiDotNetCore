using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using JsonApiDotNetCore.Resources;
using NoEntityFrameworkExample.Data;
using NoEntityFrameworkExample.Models;

namespace NoEntityFrameworkExample.Services;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TodoItemService(
    IJsonApiOptions options, IResourceGraph resourceGraph, IQueryLayerComposer queryLayerComposer, IPaginationContext paginationContext,
    IEnumerable<IQueryConstraintProvider> constraintProviders, IQueryableBuilder queryableBuilder, ILoggerFactory loggerFactory)
    : InMemoryResourceService<TodoItem, long>(options, resourceGraph, queryLayerComposer, paginationContext, constraintProviders, queryableBuilder,
        loggerFactory)
{
    protected override IEnumerable<IIdentifiable> GetDataSource(ResourceType resourceType)
    {
        if (resourceType.ClrType == typeof(TodoItem))
        {
            return Database.TodoItems;
        }

        if (resourceType.ClrType == typeof(Person))
        {
            return Database.People;
        }

        if (resourceType.ClrType == typeof(Tag))
        {
            return Database.Tags;
        }

        throw new InvalidOperationException($"Unknown data source '{resourceType.ClrType}'.");
    }
}
