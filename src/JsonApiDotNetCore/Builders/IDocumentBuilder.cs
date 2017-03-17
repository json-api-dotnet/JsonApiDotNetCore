using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Builders
{
    public interface IDocumentBuilder
    {
        Document Build(IIdentifiable entity);
        Documents Build(IEnumerable<IIdentifiable> entities);
    }
}