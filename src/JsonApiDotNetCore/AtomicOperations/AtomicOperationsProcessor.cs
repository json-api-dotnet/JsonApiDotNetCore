using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.AtomicOperations
{
    /// <inheritdoc />
    public class AtomicOperationsProcessor : IAtomicOperationsProcessor
    {
        private readonly IAtomicOperationProcessorResolver _resolver;
        private readonly ILocalIdTracker _localIdTracker;
        private readonly IJsonApiRequest _request;
        private readonly ITargetedFields _targetedFields;
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly DbContext _dbContext;

        public AtomicOperationsProcessor(IAtomicOperationProcessorResolver resolver,
            ILocalIdTracker localIdTracker, IJsonApiRequest request, ITargetedFields targetedFields,
            IResourceContextProvider resourceContextProvider, IEnumerable<IDbContextResolver> dbContextResolvers)
        {
            if (dbContextResolvers == null) throw new ArgumentNullException(nameof(dbContextResolvers));

            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _localIdTracker = localIdTracker ?? throw new ArgumentNullException(nameof(localIdTracker));
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _targetedFields = targetedFields ?? throw new ArgumentNullException(nameof(targetedFields));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));

            var resolvers = dbContextResolvers.ToArray();
            if (resolvers.Length != 1)
            {
                throw new InvalidOperationException(
                    "TODO: @OPS: At least one DbContext is required for atomic operations. Multiple DbContexts are currently not supported.");
            }

            _dbContext = resolvers[0].GetContext();
        }

        public async Task<IList<IIdentifiable>> ProcessAsync(IList<OperationContainer> operations, CancellationToken cancellationToken)
        {
            if (operations == null) throw new ArgumentNullException(nameof(operations));

            // TODO: @OPS: Consider to validate local:id usage upfront.

            var results = new List<IIdentifiable>();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                foreach (var operation in operations)
                {
                    _dbContext.ResetChangeTracker();

                    var result = await ProcessOperation(operation, cancellationToken);
                    results.Add(result);
                }

                await transaction.CommitAsync(cancellationToken);
                return results;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (JsonApiException exception)
            {
                if (_dbContext.Database.CurrentTransaction != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }

                foreach (var error in exception.Errors)
                {
                    error.Source.Pointer = $"/atomic:operations[{results.Count}]" + error.Source.Pointer;
                }

                throw;
            }
            catch (Exception exception)
            {
                if (_dbContext.Database.CurrentTransaction != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }

                throw new JsonApiException(new Error(HttpStatusCode.InternalServerError)
                {
                    Title = "An unhandled error occurred while processing an operation in this request.",
                    Detail = exception.Message,
                    Source =
                    {
                        Pointer = $"/atomic:operations[{results.Count}]"
                    }

                }, exception);
            }
        }

        private async Task<IIdentifiable> ProcessOperation(OperationContainer operation, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TrackLocalIds(operation);

            _targetedFields.Attributes = operation.TargetedFields.Attributes;
            _targetedFields.Relationships = operation.TargetedFields.Relationships;
                
            _request.CopyFrom(operation.Request);
            
            var processor = _resolver.ResolveProcessor(operation);
            return await processor.ProcessAsync(operation, cancellationToken);
        }

        private void TrackLocalIds(OperationContainer operation)
        {
            if (operation.Kind == OperationKind.CreateResource)
            {
                DeclareLocalId(operation.Resource);
            }
            else
            {
                AssignStringId(operation.Resource);
            }

            foreach (var relationship in operation.TargetedFields.Relationships)
            {
                var rightValue = relationship.GetValue(operation.Resource);

                foreach (var rightResource in TypeHelper.ExtractResources(rightValue))
                {
                    AssignStringId(rightResource);
                }
            }
        }

        private void DeclareLocalId(IIdentifiable resource)
        {
            if (resource.LocalId != null)
            {
                var resourceContext = _resourceContextProvider.GetResourceContext(resource.GetType());
                _localIdTracker.Declare(resource.LocalId, resourceContext.PublicName);
            }
        }

        private void AssignStringId(IIdentifiable resource)
        {
            if (resource.LocalId != null)
            {
                var resourceContext = _resourceContextProvider.GetResourceContext(resource.GetType());
                resource.StringId = _localIdTracker.GetValue(resource.LocalId, resourceContext.PublicName);
            }
        }
    }
}
