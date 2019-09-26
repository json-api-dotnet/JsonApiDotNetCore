using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IInternalFieldsQueryService
    {
        void Register(AttrAttribute selected, RelationshipAttribute relationship = null);
    }
}