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
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _targetedFields = targetedFields ?? throw new ArgumentNullException(nameof(targetedFields));
            _resourceGraph = resourceGraph ?? throw new ArgumentNullException(nameof(resourceGraph));

            if (dbContextResolvers == null) throw new ArgumentNullException(nameof(dbContextResolvers));

            var resolvers = dbContextResolvers.ToArray();
            if (resolvers.Length != 1)
            {
                throw new InvalidOperationException(
                    "TODO: At least one DbContext is required for atomic operations. Multiple DbContexts are currently not supported.");
            }

            _dbContext = resolvers[0].GetContext();
        }

        public async Task<IList<AtomicOperation>> ProcessAsync(IList<AtomicOperation> operations, CancellationToken cancellationToken)
        {
            if (operations == null) throw new ArgumentNullException(nameof(operations));

            var outputOps = new List<AtomicOperation>();
            var opIndex = 0;
            AtomicOperationCode? lastAttemptedOperation = null; // used for error messages only

            using (var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    foreach (var operation in operations)
                    {
                        lastAttemptedOperation = operation.Code;
                        await ProcessOperation(operation, outputOps, cancellationToken);
                        opIndex++;
                    }

                    await transaction.CommitAsync(cancellationToken);
                    return outputOps;
                }
                catch (JsonApiException exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw new JsonApiException(new Error(exception.Error.StatusCode)
                    {
                        Title = "Transaction failed on operation.",
                        Detail = $"Transaction failed on operation[{opIndex}] ({lastAttemptedOperation})."
                    }, exception);
                }
                catch (Exception exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw new JsonApiException(new Error(HttpStatusCode.InternalServerError)
                    {
                        Title = "Transaction failed on operation.",
                        Detail = $"Transaction failed on operation[{opIndex}] ({lastAttemptedOperation}) for an unexpected reason."
                    }, exception);
                }
            }
        }

        private async Task ProcessOperation(AtomicOperation inputOperation, List<AtomicOperation> outputOperations, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ReplaceLocalIdsInResourceObject(inputOperation.SingleData, outputOperations);
            ReplaceLocalIdsInRef(inputOperation.Ref, outputOperations);

            string type = null;
            if (inputOperation.Code == AtomicOperationCode.Add || inputOperation.Code == AtomicOperationCode.Update)
            {
                type = inputOperation.SingleData.Type;
            }
            else if (inputOperation.Code == AtomicOperationCode.Remove)
            {
                type = inputOperation.Ref.Type;
            }

            ((JsonApiRequest)_request).PrimaryResource = _resourceGraph.GetResourceContext(type);
            
            _targetedFields.Attributes.Clear();
            _targetedFields.Relationships.Clear();

            var processor = GetOperationsProcessor(inputOperation);
            var resultOp = await processor.ProcessAsync(inputOperation, cancellationToken);

            if (resultOp != null)
                outputOperations.Add(resultOp);
        }

        private void ReplaceLocalIdsInResourceObject(ResourceObject resourceObject, List<AtomicOperation> outputOperations)
        {
            if (resourceObject == null)
                return;

            // it is strange to me that a top level resource object might use a lid.
            // by not replacing it, we avoid a case where the first operation is an 'add' with an 'lid'
            // and we would be unable to locate the matching 'lid' in 'outputOperations'
            //
            // we also create a scenario where I might try to update a resource I just created
            // in this case, the 'data.id' will be null, but the 'ref.id' will be replaced by the correct 'id' from 'outputOperations'
            // 
            // if(HasLocalId(resourceObject))
            //     resourceObject.Id = GetIdFromLocalId(outputOperations, resourceObject.LocalId);

            if (resourceObject.Relationships != null)
            {
                foreach (var relationshipDictionary in resourceObject.Relationships)
                {
                    if (relationshipDictionary.Value.IsManyData)
                    {
                        foreach (var relationship in relationshipDictionary.Value.ManyData)
                            if (HasLocalId(relationship))
                                relationship.Id = GetIdFromLocalId(outputOperations, relationship.LocalId);
                    }
                    else
                    {
                        var relationship = relationshipDictionary.Value.SingleData;
                        if (HasLocalId(relationship))
                            relationship.Id = GetIdFromLocalId(outputOperations, relationship.LocalId);
                    }
                }
            }
        }

        private void ReplaceLocalIdsInRef(ResourceReference resourceReference, List<AtomicOperation> outputOperations)
        {
            if (resourceReference == null) return;
            if (HasLocalId(resourceReference))
                resourceReference.Id = GetIdFromLocalId(outputOperations, resourceReference.LocalId);
        }

        private bool HasLocalId(ResourceIdentifierObject rio) => string.IsNullOrEmpty(rio.LocalId) == false;

        private string GetIdFromLocalId(List<AtomicOperation> outputOps, string localId)
        {
            var referencedOp = outputOps.FirstOrDefault(o => o.SingleData.LocalId == localId);
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

        private IAtomicOperationProcessor GetOperationsProcessor(AtomicOperation operation)
        {
            switch (operation.Code)
            {
                case AtomicOperationCode.Add:
                    return _resolver.ResolveCreateProcessor(operation);
                case AtomicOperationCode.Remove:
                    return _resolver.ResolveRemoveProcessor(operation);
                case AtomicOperationCode.Update:
                    return _resolver.ResolveUpdateProcessor(operation);
                default:
                    throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                    {
                        Title = "Invalid operation code.",
                        Detail = $"'{operation.Code}' is not a valid operation code."
                    });
            }
        }
    }
}
