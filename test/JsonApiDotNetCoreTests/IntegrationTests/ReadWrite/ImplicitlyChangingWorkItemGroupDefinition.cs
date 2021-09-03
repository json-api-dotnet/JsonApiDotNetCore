using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite
{
    /// <summary>
    /// Used to simulate side effects that occur in the database while saving, typically caused by database triggers.
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class ImplicitlyChangingWorkItemGroupDefinition : JsonApiResourceDefinition<WorkItemGroup, Guid>
    {
        internal const string Suffix = " (changed)";

        private readonly ReadWriteDbContext _dbContext;

        public ImplicitlyChangingWorkItemGroupDefinition(IResourceGraph resourceGraph, ReadWriteDbContext dbContext)
            : base(resourceGraph)
        {
            _dbContext = dbContext;
        }

        public override async Task OnWriteSucceededAsync(WorkItemGroup resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            if (writeOperation is not WriteOperationKind.DeleteResource)
            {
                string statement = "Update \"Groups\" SET \"Name\" = '" + resource.Name + Suffix + "' WHERE \"Id\" = '" + resource.Id + "'";
                await _dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
            }
        }
    }
}
