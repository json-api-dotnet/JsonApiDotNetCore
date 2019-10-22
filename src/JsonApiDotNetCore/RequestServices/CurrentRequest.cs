using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Managers
{
    class CurrentRequest : ICurrentRequest
    {
        private ResourceContext _contextEntity;
        public string BasePath { get; set; }
        public bool IsRelationshipPath { get; set; }
        public RelationshipAttribute RequestRelationship { get; set; }

        /// <summary>
        /// The main resource of the request.
        /// </summary>
        /// <returns></returns>
        public ResourceContext GetRequestResource()
        {
            return _contextEntity;
        }

        public void SetRequestResource(ResourceContext primaryResource)
        {
            _contextEntity = primaryResource;
        }
    }
}
