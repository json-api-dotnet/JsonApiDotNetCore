using System.Collections.Generic;
using System.Threading.Tasks;
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

            foreach(var op in inputOps)
            {
                // TODO: parse pointers:
                // locate all objects within the document and replace them
                var operationsPointer = new OperationsPointer();
                var replacer = new DocumentDataPointerReplacement<OperationsPointer, Operation>(op.DataObject);
                replacer.ReplacePointers(outputOps);

                /// 
                var processor = _processorResolver.LocateCreateService(op);
                var resultOp = await processor.ProcessAsync(op);
                outputOps.Add(resultOp);
            }
            for(var i=0; i < inputOps.Count; i++)
            {
                
            }
        }
    }
}
