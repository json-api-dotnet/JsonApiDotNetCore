using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            DbContext dbContext)
        {
            _processorResolver = processorResolver;
            _dbContext = dbContext;
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
            ReplaceRefPointers(op.Ref, outputOps);

            var processor = GetOperationsProcessor(op);
            var resultOp = await processor.ProcessAsync(op);

            if (resultOp != null)
                outputOps.Add(resultOp);
        }

        private void ReplaceDataPointers(DocumentData dataObject, List<Operation> outputOps)
        {
            if (dataObject == null) return;

            var replacer = new DocumentDataPointerReplacement<OperationsPointer, Operation>(dataObject);
            replacer.ReplacePointers(outputOps);
        }

        private void ReplaceRefPointers(ResourceReference resourceRef, List<Operation> outputOps)
        {
            if (resourceRef == null) return;

            var replacer = new ResourceRefPointerReplacement<OperationsPointer, Operation>(resourceRef);
            replacer.ReplacePointers(outputOps);
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
