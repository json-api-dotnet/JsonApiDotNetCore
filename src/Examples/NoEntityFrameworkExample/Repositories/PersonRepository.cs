using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using Microsoft.EntityFrameworkCore.Metadata;
using NoEntityFrameworkExample.Data;
using NoEntityFrameworkExample.Models;

namespace NoEntityFrameworkExample.Repositories;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class PersonRepository(IResourceGraph resourceGraph, IQueryableBuilder queryableBuilder, IReadOnlyModel entityModel)
    : InMemoryResourceRepository<Person, long>(resourceGraph, queryableBuilder, entityModel)
{
    protected override IEnumerable<Person> GetDataSource()
    {
        return Database.People;
    }
}
