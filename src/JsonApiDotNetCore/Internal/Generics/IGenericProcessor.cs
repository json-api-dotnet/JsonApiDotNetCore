using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Internal.Generics
{
    public interface IGenericProcessor
    {
        Task UpdateRelationshipsAsync(object parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds);
        void SetRelationships(object parent, RelationshipAttribute relationship, IEnumerable<string> relationshipIds);
    }
}
