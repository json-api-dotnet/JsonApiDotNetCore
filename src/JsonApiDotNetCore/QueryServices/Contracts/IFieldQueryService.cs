using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IFieldsQueryService
    {
        List<AttrAttribute> Get(RelationshipAttribute relationship = null);
    }
}