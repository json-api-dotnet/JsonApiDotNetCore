using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Managers
{
    class CurrentRequest : ICurrentRequest
    {
        private ContextEntity _contextEntity;
        public string BasePath { get; set; }
        public bool IsRelationshipPath { get; set; }
        public RelationshipAttribute RequestRelationship { get; set; }

        /// <summary>
        /// The main resource of the request.
        /// </summary>
        /// <returns></returns>
        public ContextEntity GetRequestResource()
        {
            return _contextEntity;
        }

        public void SetRequestResource(ContextEntity primaryResource)
        {
            _contextEntity = primaryResource;
        }
    }
}
