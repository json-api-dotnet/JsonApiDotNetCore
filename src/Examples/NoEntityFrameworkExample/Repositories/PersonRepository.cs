using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using NoEntityFrameworkExample.Data;
using NoEntityFrameworkExample.Models;

namespace NoEntityFrameworkExample.Repositories;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class PersonRepository(IResourceGraph resourceGraph, IQueryableBuilder queryableBuilder)
    : InMemoryResourceRepository<Person, long>(resourceGraph, queryableBuilder)
{
    protected override IEnumerable<Person> GetDataSource()
    {
        return Database.People;
    }
}
