//using JsonApiDotNetCore.Internal.Contracts;
//using JsonApiDotNetCore.Models;

//namespace JsonApiDotNetCore.Serialization
//{
//    public class RequestResourceObjectBuilder : ResourceObjectBuilder, IResourceObjectBuilder
//    {
//        public RequestResourceObjectBuilder(IResourceGraph resourceGraph, IContextEntityProvider provider) : base(resourceGraph, provider, new ResourceObjectBuilderSettings()) { }

//        protected override RelationshipEntry GetRelationshipData(RelationshipAttribute relationship, IIdentifiable entity)
//        {
//            return new RelationshipEntry { Data = GetRelatedResourceLinkage(relationship, entity) };
//        }
//    }
//}
