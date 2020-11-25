using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.AtomicOperations.Processors;
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
        private readonly DbContext _dbContext;
        private readonly IJsonApiRequest _request;
        private readonly ITargetedFields _targetedFields;
        private readonly IResourceGraph _resourceGraph;

        public AtomicOperationsProcessor(IAtomicOperationProcessorResolver resolver, IJsonApiRequest request,
            ITargetedFields targetedFields, IResourceGraph resourceGraph,
            IEnumerable<IDbContextResolver> dbContextResolvers)
        {
            if (dbContextResolvers == null) throw new ArgumentNullException(nameof(dbContextResolvers));

            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _targetedFields = targetedFields ?? throw new ArgumentNullException(nameof(targetedFields));
            _resourceGraph = resourceGraph ?? throw new ArgumentNullException(nameof(resourceGraph));

            var resolvers = dbContextResolvers.ToArray();
            if (resolvers.Length != 1)
            {
                throw new InvalidOperationException(
                    "TODO: At least one DbContext is required for atomic operations. Multiple DbContexts are currently not supported.");
            }

            _dbContext = resolvers[0].GetContext();
        }

        public async Task<IList<AtomicResultObject>> ProcessAsync(IList<AtomicOperationObject> operations, CancellationToken cancellationToken)
        {
            if (operations == null) throw new ArgumentNullException(nameof(operations));

            var results = new List<AtomicResultObject>();
            var operationIndex = 0;
            AtomicOperationCode? lastAttemptedOperation = null; // used for error messages only

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                foreach (var operation in operations)
                {
                    lastAttemptedOperation = operation.Code;
                    await ProcessOperation(operation, results, cancellationToken);
                    operationIndex++;
                }

                await transaction.CommitAsync(cancellationToken);
                return results;
            }
            catch (JsonApiException exception)
            {
                await transaction.RollbackAsync(cancellationToken);

                throw new JsonApiException(new Error(exception.Error.StatusCode)
                {
                    Title = "Transaction failed on operation.",
                    Detail = $"Transaction failed on operation[{operationIndex}] ({lastAttemptedOperation})."
                }, exception);
            }
            catch (Exception exception)
            {
                await transaction.RollbackAsync(cancellationToken);

                throw new JsonApiException(new Error(HttpStatusCode.InternalServerError)
                {
                    Title = "Transaction failed on operation.",
                    Detail = $"Transaction failed on operation[{operationIndex}] ({lastAttemptedOperation}) for an unexpected reason."
                }, exception);
            }
        }

        private async Task ProcessOperation(AtomicOperationObject operation, List<AtomicResultObject> results, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ReplaceLocalIdsInResourceObject(operation.SingleData, results);
            ReplaceLocalIdsInRef(operation.Ref, results);

            string type = null;
            if (operation.Code == AtomicOperationCode.Add || operation.Code == AtomicOperationCode.Update)
            {
                type = operation.SingleData.Type;
            }
            else if (operation.Code == AtomicOperationCode.Remove)
            {
                type = operation.Ref.Type;
            }

            ((JsonApiRequest)_request).PrimaryResource = _resourceGraph.GetResourceContext(type);
            
            _targetedFields.Attributes.Clear();
            _targetedFields.Relationships.Clear();

            var processor = _resolver.ResolveProcessor(operation);
            var result = await processor.ProcessAsync(operation, cancellationToken);
            results.Add(result);
        }

        private void ReplaceLocalIdsInResourceObject(ResourceObject resourceObject, List<AtomicResultObject> results)
        {
            if (resourceObject == null)
                return;

            // it is strange to me that a top level resource object might use a lid.
            // by not replacing it, we avoid a case where the first operation is an 'add' with an 'lid'
            // and we would be unable to locate the matching 'lid' in 'results'
            //
            // we also create a scenario where I might try to update a resource I just created
            // in this case, the 'data.id' will be null, but the 'ref.id' will be replaced by the correct 'id' from 'results'
            // 
            // if(HasLocalId(resourceObject))
            //     resourceObject.Id = GetIdFromLocalId(results, resourceObject.LocalId);

            if (resourceObject.Relationships != null)
            {
                foreach (var relationshipDictionary in resourceObject.Relationships)
                {
                    if (relationshipDictionary.Value.IsManyData)
                    {
                        foreach (var relationship in relationshipDictionary.Value.ManyData)
                            if (HasLocalId(relationship))
                                relationship.Id = GetIdFromLocalId(results, relationship.LocalId);
                    }
                    else
                    {
                        var relationship = relationshipDictionary.Value.SingleData;
                        if (HasLocalId(relationship))
                            relationship.Id = GetIdFromLocalId(results, relationship.LocalId);
                    }
                }
            }
        }

        private void ReplaceLocalIdsInRef(AtomicResourceReference reference, List<AtomicResultObject> results)
        {
            if (reference == null) return;
            if (HasLocalId(reference))
                reference.Id = GetIdFromLocalId(results, reference.LocalId);
        }

        private bool HasLocalId(ResourceIdentifierObject rio) => string.IsNullOrEmpty(rio.LocalId) == false;

        private string GetIdFromLocalId(List<AtomicResultObject> results, string localId)
        {
            var referencedOp = results.FirstOrDefault(o => o.SingleData.LocalId == localId);
            if (referencedOp == null)
            {
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "Could not locate lid in document.",
                    Detail = $"Could not locate lid '{localId}' in document."
                });
            }

            return referencedOp.SingleData.Id;
        }
    }
}
