using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Transactions
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class PerformerRepository : IResourceRepository<Performer, int>
    {
        public Task<IReadOnlyCollection<Performer>> GetAsync(QueryLayer queryLayer, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountAsync(FilterExpression? filter, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Performer> GetForCreateAsync(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task CreateAsync(Performer resourceFromRequest, Performer resourceForDatabase, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Performer?> GetForUpdateAsync(QueryLayer queryLayer, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Performer resourceFromRequest, Performer resourceFromDatabase, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetRelationshipAsync(Performer leftResource, object? rightValue, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task AddToToManyRelationshipAsync(int leftId, ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveFromToManyRelationshipAsync(Performer leftResource, ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
