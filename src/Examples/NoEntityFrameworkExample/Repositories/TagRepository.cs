using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using NoEntityFrameworkExample.Data;
using NoEntityFrameworkExample.Models;

namespace NoEntityFrameworkExample.Repositories;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TagRepository : InMemoryResourceRepository<Tag, long>
{
    public TagRepository(IResourceGraph resourceGraph, IQueryableBuilder queryableBuilder)
        : base(resourceGraph, queryableBuilder)
    {
    }

    protected override IEnumerable<Tag> GetDataSource()
    {
        return Database.Tags;
    }
}
