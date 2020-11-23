using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Services.Operations
{
    public interface IOperationsProcessor
    {
        Task<List<Operation>> ProcessAsync(List<Operation> inputOps, CancellationToken cancellationToken);
    }

    public class OperationsProcessor : IOperationsProcessor
    {
        private readonly IOperationProcessorResolver _processorResolver;
        private readonly DbContext _dbContext;
        private readonly IJsonApiRequest _request;
        private readonly ITargetedFields _targetedFields;
        private readonly IResourceGraph _resourceGraph;

        public OperationsProcessor(
            IOperationProcessorResolver processorResolver,
            IDbContextResolver dbContextResolver,
            IJsonApiRequest request,
            ITargetedFields targetedFields,
            IResourceGraph resourceGraph)
        {
            _processorResolver = processorResolver;
            _dbContext = dbContextResolver.GetContext();
            _request = request;
            _targetedFields = targetedFields;
            _resourceGraph = resourceGraph;
        }

        public async Task<List<Operation>> ProcessAsync(List<Operation> inputOps, CancellationToken cancellationToken)
        {
            var outputOps = new List<Operation>();
            var opIndex = 0;
            OperationCode? lastAttemptedOperation = null; // used for error messages only

            using (var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    foreach (var op in inputOps)
                    {
                        lastAttemptedOperation = op.Op;
                        await ProcessOperation(op, outputOps, cancellationToken);
                        opIndex++;
                    }

                    await transaction.CommitAsync(cancellationToken);
                    return outputOps;
                }
                catch (JsonApiException e)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw new JsonApiException(new Error(e.Error.StatusCode)
                    {
                        Title = "Transaction failed on operation.",
                        Detail = $"Transaction failed on operation[{opIndex}] ({lastAttemptedOperation})."
                    }, e);
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw new JsonApiException(new Error(HttpStatusCode.InternalServerError)
                    {
                        Title = "Transaction failed on operation.",
                        Detail = $"Transaction failed on operation[{opIndex}] ({lastAttemptedOperation}) for an unexpected reason."
                    }, e);
                }
            }
        }

        private async Task ProcessOperation(Operation op, List<Operation> outputOps, CancellationToken cancellationToken)
        {
            ReplaceLocalIdsInResourceObject(op.DataObject, outputOps);
            ReplaceLocalIdsInRef(op.Ref, outputOps);

            string type = null;
            if (op.Op == OperationCode.add || op.Op == OperationCode.update)
            {
                type = op.DataObject.Type;
            }
            else if (op.Op == OperationCode.remove)
            {
                type = op.Ref.Type;
            }

            ((JsonApiRequest)_request).PrimaryResource = _resourceGraph.GetResourceContext(type);
            
            _targetedFields.Attributes.Clear();
            _targetedFields.Relationships.Clear();

            var processor = GetOperationsProcessor(op);
            var resultOp = await processor.ProcessAsync(op, cancellationToken);

            if (resultOp != null)
                outputOps.Add(resultOp);
        }

        private void ReplaceLocalIdsInResourceObject(ResourceObject resourceObject, List<Operation> outputOps)
        {
            if (resourceObject == null)
                return;

            // it is strange to me that a top level resource object might use a lid.
            // by not replacing it, we avoid a case where the first operation is an 'add' with an 'lid'
            // and we would be unable to locate the matching 'lid' in 'outputOps'
            //
            // we also create a scenario where I might try to update a resource I just created
            // in this case, the 'data.id' will be null, but the 'ref.id' will be replaced by the correct 'id' from 'outputOps'
            // 
            // if(HasLocalId(resourceObject))
            //     resourceObject.Id = GetIdFromLocalId(outputOps, resourceObject.LocalId);

            if (resourceObject.Relationships != null)
            {
                foreach (var relationshipDictionary in resourceObject.Relationships)
                {
                    if (relationshipDictionary.Value.IsManyData)
                    {
                        foreach (var relationship in relationshipDictionary.Value.ManyData)
                            if (HasLocalId(relationship))
                                relationship.Id = GetIdFromLocalId(outputOps, relationship.LocalId);
                    }
                    else
                    {
                        var relationship = relationshipDictionary.Value.SingleData;
                        if (HasLocalId(relationship))
                            relationship.Id = GetIdFromLocalId(outputOps, relationship.LocalId);
                    }
                }
            }
        }

        private void ReplaceLocalIdsInRef(ResourceReference resourceRef, List<Operation> outputOps)
        {
            if (resourceRef == null) return;
            if (HasLocalId(resourceRef))
                resourceRef.Id = GetIdFromLocalId(outputOps, resourceRef.LocalId);
        }

        private bool HasLocalId(ResourceIdentifierObject rio) => string.IsNullOrEmpty(rio.LocalId) == false;

        private string GetIdFromLocalId(List<Operation> outputOps, string localId)
        {
            var referencedOp = outputOps.FirstOrDefault(o => o.DataObject.LocalId == localId);
            if (referencedOp == null)
            {
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "Could not locate lid in document.",
                    Detail = $"Could not locate lid '{localId}' in document."
                });
            }

            return referencedOp.DataObject.Id;
        }

        private IOpProcessor GetOperationsProcessor(Operation op)
        {
            switch (op.Op)
            {
                case OperationCode.add:
                    return _processorResolver.LocateCreateService(op);
                case OperationCode.remove:
                    return _processorResolver.LocateRemoveService(op);
                case OperationCode.update:
                    return _processorResolver.LocateUpdateService(op);
                default:
                    throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                    {
                        Title = "Invalid operation code.",
                        Detail = $"'{op.Op}' is not a valid operation code."
                    });
            }
        }
    }
}
