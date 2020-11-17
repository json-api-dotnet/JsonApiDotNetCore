using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.IdObfuscation
{
    public abstract class ObfuscatedIdentifiableController<TResource> : BaseJsonApiController<TResource>
        where TResource : class, IIdentifiable<int>
    {
        protected ObfuscatedIdentifiableController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<TResource> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }

        [HttpGet]
        public override Task<IActionResult> GetAsync()
        {
            return base.GetAsync();
        }

        [HttpGet("{id}")]
        public Task<IActionResult> GetAsync(string id)
        {
            int idValue = HexadecimalCodec.Decode(id);
            return base.GetAsync(idValue);
        }

        [HttpGet("{id}/{relationshipName}")]
        public Task<IActionResult> GetSecondaryAsync(string id, string relationshipName)
        {
            int idValue = HexadecimalCodec.Decode(id);
            return base.GetSecondaryAsync(idValue, relationshipName);
        }

        [HttpGet("{id}/relationships/{relationshipName}")]
        public Task<IActionResult> GetRelationshipAsync(string id, string relationshipName)
        {
            int idValue = HexadecimalCodec.Decode(id);
            return base.GetRelationshipAsync(idValue, relationshipName);
        }

        [HttpPost]
        public override Task<IActionResult> PostAsync([FromBody] TResource resource)
        {
            return base.PostAsync(resource);
        }

        [HttpPost("{id}/relationships/{relationshipName}")]
        public Task<IActionResult> PostRelationshipAsync(string id, string relationshipName,
            [FromBody] ISet<IIdentifiable> secondaryResourceIds)
        {
            int idValue = HexadecimalCodec.Decode(id);
            return base.PostRelationshipAsync(idValue, relationshipName, secondaryResourceIds);
        }

        [HttpPatch("{id}")]
        public Task<IActionResult> PatchAsync(string id, [FromBody] TResource resource)
        {
            int idValue = HexadecimalCodec.Decode(id);
            return base.PatchAsync(idValue, resource);
        }

        [HttpPatch("{id}/relationships/{relationshipName}")]
        public Task<IActionResult> PatchRelationshipAsync(string id, string relationshipName,
            [FromBody] object secondaryResourceIds)
        {
            int idValue = HexadecimalCodec.Decode(id);
            return base.PatchRelationshipAsync(idValue, relationshipName, secondaryResourceIds);
        }

        [HttpDelete("{id}")]
        public Task<IActionResult> DeleteAsync(string id)
        {
            int idValue = HexadecimalCodec.Decode(id);
            return base.DeleteAsync(idValue);
        }

        [HttpDelete("{id}/relationships/{relationshipName}")]
        public Task<IActionResult> DeleteRelationshipAsync(string id, string relationshipName,
            [FromBody] ISet<IIdentifiable> secondaryResourceIds)
        {
            int idValue = HexadecimalCodec.Decode(id);
            return base.DeleteRelationshipAsync(idValue, relationshipName, secondaryResourceIds);
        }
    }
}
