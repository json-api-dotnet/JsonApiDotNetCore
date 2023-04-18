using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Resources;
using NoEntityFrameworkExample.Data;
using NoEntityFrameworkExample.Models;

namespace NoEntityFrameworkExample.Services;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TodoItemService : InMemoryResourceService<TodoItem, long>
{
    public TodoItemService(IJsonApiOptions options, IResourceGraph resourceGraph, IQueryLayerComposer queryLayerComposer, IResourceFactory resourceFactory,
        IPaginationContext paginationContext, IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory)
        : base(options, resourceGraph, queryLayerComposer, resourceFactory, paginationContext, constraintProviders, loggerFactory)
    {
    }

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
