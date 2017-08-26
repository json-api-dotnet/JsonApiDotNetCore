using System.Threading.Tasks;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Operations;

namespace JsonApiDotNetCore.Services.Operations
{
    public interface IOpProcessor
    {
        Task<Operation> ProcessAsync(Operation operation);
    }

    public interface IOpProcessor<T, TId> : IOpProcessor 
        where T : class, IIdentifiable<TId>
    { }
}
