using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCore.Models.Pointers;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Services.Operations
{
    public interface IOperationsProcessor
    {
        Task<List<Operation>> ProcessAsync(List<Operation> inputOps);
    }

    public class OperationsProcessor : IOperationsProcessor
    {
        private readonly IOperationProcessorResolver _processorResolver;
        private readonly DbContext _dbContext;

        public OperationsProcessor(
            IOperationProcessorResolver processorResolver,
            IDbContextResolver dbContextResolver)
        {
            _processorResolver = processorResolver;
            _dbContext = dbContextResolver.GetContext();
        }

        public async Task<List<Operation>> ProcessAsync(List<Operation> inputOps)
        {
            var outputOps = new List<Operation>();
            var opIndex = 0;

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var op in inputOps)
                    {
                        await ProcessOperation(op, outputOps);
                        opIndex++;
                    }

                    transaction.Commit();
                }
                catch (JsonApiException e)
                {
                    outputOps = new List<Operation>();
                    throw new JsonApiException(e.GetStatusCode(), $"Transaction failed on operation[{opIndex}].", e);
                }
                catch (Exception e)
                {
                    throw new JsonApiException(500, $"Transaction failed on operation[{opIndex}] for an unexpected reason.", e);
                }
            }

            return outputOps;
        }

        private async Task ProcessOperation(Operation op, List<Operation> outputOps)
        {
            var operationsPointer = new OperationsPointer();

            ReplaceLocalIdsInResourceObject(op.DataObject, outputOps);
            ReplaceLocalIdsInRef(op.Ref, outputOps);

            var processor = GetOperationsProcessor(op);
            var resultOp = await processor.ProcessAsync(op);

            if (resultOp != null)
                outputOps.Add(resultOp);
        }

        private void ReplaceLocalIdsInResourceObject(ResourceObject resourceObject, List<Operation> outputOps)
        {
            if (resourceObject == null) return;

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
                    if (relationshipDictionary.Value.IsHasMany) 
                    { 
                        foreach (var relationship in relationshipDictionary.Value.ManyData) 
                            if(HasLocalId(relationship))
                                relationship.Id = GetIdFromLocalId(outputOps, relationship.LocalId);
                    } 
                    else
                    {
                        var relationship = relationshipDictionary.Value.SingleData;
                        if(HasLocalId(relationship))
                            relationship.Id = GetIdFromLocalId(outputOps, relationship.LocalId);
                    }
                } 
            }
        }

        private void ReplaceLocalIdsInRef(ResourceReference resourceRef, List<Operation> outputOps) 
        { 
            if (resourceRef == null) return;
            if(HasLocalId(resourceRef))
                resourceRef.Id = GetIdFromLocalId(outputOps, resourceRef.LocalId);
        }

        private bool HasLocalId(ResourceIdentifierObject rio) => string.IsNullOrEmpty(rio.LocalId) == false;

        private string GetIdFromLocalId(List<Operation> outputOps, string localId)  
        {
            var referencedOp = outputOps.FirstOrDefault(o => o.DataObject.LocalId == localId);
            if(referencedOp == null) throw new JsonApiException(400, $"Could not locate lid '{localId}' in document.");
            return referencedOp.DataObject.Id;
        }

        private IOpProcessor GetOperationsProcessor(Operation op)
        {
            switch (op.Op)
            {
                case OperationCode.add:
                    return _processorResolver.LocateCreateService(op);
                case OperationCode.get:
                    return _processorResolver.LocateGetService(op);
                case OperationCode.replace:
                    return _processorResolver.LocateReplaceService(op);
                case OperationCode.remove:
                    return _processorResolver.LocateRemoveService(op);
                default:
                    throw new JsonApiException(400, $"'{op.Op}' is not a valid operation code");
            }
        }
    }
}
