using System.Threading.Tasks;
using JsonApiDotNetCore.Models.Operations;

namespace JsonApiDotNetCore.Services.Operations
{
    public interface IOpProcessor
    {
        Task<Operation> ProcessAsync(Operation operation);
    }
}
