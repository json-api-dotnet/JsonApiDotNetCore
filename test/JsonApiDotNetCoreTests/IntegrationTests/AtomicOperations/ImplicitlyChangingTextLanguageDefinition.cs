using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations
{
    /// <summary>
    /// Used to simulate side effects that occur in the database while saving, typically caused by database triggers.
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public class ImplicitlyChangingTextLanguageDefinition : JsonApiResourceDefinition<TextLanguage, Guid>
    {
        internal const string Suffix = " (changed)";

        private readonly OperationsDbContext _dbContext;

        public ImplicitlyChangingTextLanguageDefinition(IResourceGraph resourceGraph, OperationsDbContext dbContext)
            : base(resourceGraph)
        {
            _dbContext = dbContext;
        }

        public override async Task OnWriteSucceededAsync(TextLanguage resource, WriteOperationKind writeOperation, CancellationToken cancellationToken)
        {
            if (writeOperation is not WriteOperationKind.DeleteResource)
            {
                string statement = $"Update \"TextLanguages\" SET \"IsoCode\" = '{resource.IsoCode}{Suffix}' WHERE \"Id\" = '{resource.StringId}'";
                await _dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
            }
        }
    }
}
