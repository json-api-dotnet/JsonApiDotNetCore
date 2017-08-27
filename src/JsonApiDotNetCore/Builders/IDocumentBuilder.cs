using System.Collections.Generic;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Builders
{
    public interface IDocumentBuilder
    {
        Document Build(IIdentifiable entity);
        Documents Build(IEnumerable<IIdentifiable> entities);
        DocumentData GetData(ContextEntity contextEntity, IIdentifiable entity);
    }
}