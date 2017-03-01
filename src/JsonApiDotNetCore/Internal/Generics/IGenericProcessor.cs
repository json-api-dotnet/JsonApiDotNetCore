using System.Collections.Generic;
using System.Threading.Tasks;

namespace JsonApiDotNetCore.Internal
{
    public interface IGenericProcessor
    {
        Task UpdateRelationshipsAsync(object parent, Relationship relationship, IEnumerable<string> relationshipIds);
    }
}
