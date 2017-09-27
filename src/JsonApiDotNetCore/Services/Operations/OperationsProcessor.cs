using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCore.Models.Pointers;

namespace JsonApiDotNetCore.Services.Operations
{
    public interface IOperationsProcessor
    {
        Task<List<Operation>> ProcessAsync(List<Operation> inputOps);
    }

    public class OperationsProcessor : IOperationsProcessor
    {
        private readonly IOperationProcessorResolver _processorResolver;

        public OperationsProcessor(IOperationProcessorResolver processorResolver)
        {
            _processorResolver = processorResolver;
        }

        public async Task<List<Operation>> ProcessAsync(List<Operation> inputOps)
        {
            var outputOps = new List<Operation>();

            foreach (var op in inputOps)
            {
                // TODO: parse pointers:
                // locate all objects within the document and replace them
                var operationsPointer = new OperationsPointer();

                ReplaceDataPointers(op.DataObject, outputOps);
                ReplaceRefPointers(op.Ref, outputOps);

                var processor = GetOperationsProcessor(op);
                var resultOp = await processor.ProcessAsync(op);

                if (resultOp != null)
                    outputOps.Add(resultOp);
            }

            return outputOps;
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
                    return _processorResolver.LocateGeteService(op);
                default:
                    throw new JsonApiException(400, $"'{op.Op}' is not a valid operation code");
            }
        }
    }
}
