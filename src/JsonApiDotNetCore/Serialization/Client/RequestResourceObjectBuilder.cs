using System;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Serialization
{
    public class RequestResourceObjectBuilder : BaseResourceObjectBuilder, IResourceObjectBuilder
    {
        public RequestResourceObjectBuilder(IResourceGraph resourceGraph, IContextEntityProvider provider, ResourceObjectBuilderSettings settings) : base(resourceGraph, provider, settings)
        {
        }

        protected override RelationshipEntry GetRelationshipData(RelationshipAttribute relationship, IIdentifiable entity)
        {
            return new RelationshipEntry { Data = GetRelatedResourceLinkage(relationship, entity) };
        }
    }
}
