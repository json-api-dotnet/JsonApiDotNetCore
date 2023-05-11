using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using NoEntityFrameworkExample.Data;
using NoEntityFrameworkExample.Models;

namespace NoEntityFrameworkExample.Repositories;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class TagRepository : InMemoryResourceRepository<Tag, long>
{
    public TagRepository(IResourceGraph resourceGraph, IResourceFactory resourceFactory)
        : base(resourceGraph, resourceFactory)
    {
    }

    protected override IEnumerable<Tag> GetDataSource()
    {
        return Database.Tags;
    }
}
