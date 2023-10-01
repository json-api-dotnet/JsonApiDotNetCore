using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.QueryableBuilding;
using NoEntityFrameworkExample.Data;
using NoEntityFrameworkExample.Models;

namespace NoEntityFrameworkExample.Repositories;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class PersonRepository : InMemoryResourceRepository<Person, long>
{
    public PersonRepository(IResourceGraph resourceGraph, IQueryableBuilder queryableBuilder)
        : base(resourceGraph, queryableBuilder)
    {
    }

    protected override IEnumerable<Person> GetDataSource()
    {
        return Database.People;
    }
}
