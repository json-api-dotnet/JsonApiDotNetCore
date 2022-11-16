using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Builders
{
    public interface IDocumentBuilder
    {
        Document Build(IIdentifiable entity);
        Documents Build(IEnumerable<IIdentifiable> entities);

        [Obsolete("You should specify an IResourceDefinition implementation using the GetData/3 overload.")]
        ResourceObject GetData(ContextEntity contextEntity, IIdentifiable entity);
        ResourceObject GetData(ContextEntity contextEntity, IIdentifiable entity, IResourceDefinition resourceDefinition = null);
    }
}
