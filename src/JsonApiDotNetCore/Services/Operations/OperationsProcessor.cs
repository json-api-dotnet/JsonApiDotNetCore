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

            ReplaceDataPointers(op.DataObject, outputOps);
            // ReplaceRefPointers(op.Ref, outputOps);

            var processor = GetOperationsProcessor(op);
            var resultOp = await processor.ProcessAsync(op);

            if (resultOp != null)
                outputOps.Add(resultOp);
        }

        private void ReplaceDataPointers(DocumentData data, List<Operation> outputOps)
        {
            if (data == null) return;

            bool HasLocalId(ResourceIdentifierObject rio) => string.IsNullOrEmpty(rio.LocalId) == false;
            string GetIdFromLocalId(string localId)  {
                var referencedOp = outputOps.FirstOrDefault(o => o.DataObject.LocalId == localId);
                if(referencedOp == null) throw new JsonApiException(400, $"Could not locate lid '{localId}' in document.");
                return referencedOp.DataObject.Id;
            };

            // are there any circumstances where the primary data would contain an lid?
            // if(HasLocalId(data))
            // {
            //     data.Id = GetIdFromLocalId(data.LocalId);
            // }

            if (data.Relationships != null) 
            { 
                foreach (var relationshipDictionary in data.Relationships) 
                { 
                    if (relationshipDictionary.Value.IsHasMany) 
                    { 
                        foreach (var relationship in relationshipDictionary.Value.ManyData) 
                            if(HasLocalId(relationship))
                                relationship.Id = GetIdFromLocalId(relationship.LocalId);
                    } 
                    else
                    {
                        var relationship = relationshipDictionary.Value.SingleData;
                        if(HasLocalId(relationship))
                            relationship.Id = GetIdFromLocalId(relationship.LocalId);
                    }
                } 
            }
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
