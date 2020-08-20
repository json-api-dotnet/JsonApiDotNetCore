using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Controllers
{
    public abstract class JsonApiCommandController<TResource> : JsonApiCommandController<TResource, int> where TResource : class, IIdentifiable<int>
    {
        protected JsonApiCommandController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceCommandService<TResource, int> commandService)
            : base(options, loggerFactory, commandService)
        { }
    }

    public abstract class JsonApiCommandController<TResource, TId> : BaseJsonApiController<TResource, TId> where TResource : class, IIdentifiable<TId>
    {
        protected JsonApiCommandController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceCommandService<TResource, TId> commandService)
            : base(options, loggerFactory, null, commandService)
        { }

        [HttpPost]
        public override async Task<IActionResult> PostAsync([FromBody] TResource resource)
            => await base.PostAsync(resource);

        [HttpPatch("{id}")]
        public override async Task<IActionResult> PatchAsync(TId id, [FromBody] TResource resource)
            => await base.PatchAsync(id, resource);

        [HttpPatch("{id}/relationships/{relationshipName}")]
        public override async Task<IActionResult> PatchRelationshipAsync(
            TId id, string relationshipName, [FromBody] object relationships)
            => await base.PatchRelationshipAsync(id, relationshipName, relationships);

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(TId id) => await base.DeleteAsync(id);
    }
}
