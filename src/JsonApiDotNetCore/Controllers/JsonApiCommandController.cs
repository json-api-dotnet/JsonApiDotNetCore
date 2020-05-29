using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Controllers
{
    public abstract class JsonApiCommandController<T> : JsonApiCommandController<T, int> where T : class, IIdentifiable<int>
    {
        protected JsonApiCommandController(
            IJsonApiOptions jsonApiOptions,
            ILoggerFactory loggerFactory,
            IResourceCommandService<T, int> commandService)
            : base(jsonApiOptions, loggerFactory, commandService)
        { }
    }

    public abstract class JsonApiCommandController<T, TId> : BaseJsonApiController<T, TId> where T : class, IIdentifiable<TId>
    {
        protected JsonApiCommandController(
            IJsonApiOptions jsonApiOptions,
            ILoggerFactory loggerFactory,
            IResourceCommandService<T, TId> commandService)
            : base(jsonApiOptions, loggerFactory, null, commandService)
        { }

        [HttpPost]
        public override async Task<IActionResult> PostAsync([FromBody] T resource)
            => await base.PostAsync(resource);

        [HttpPatch("{id}")]
        public override async Task<IActionResult> PatchAsync(TId id, [FromBody] T resource)
            => await base.PatchAsync(id, resource);

        [HttpPatch("{id}/relationships/{relationshipName}")]
        public override async Task<IActionResult> PatchRelationshipAsync(
            TId id, string relationshipName, [FromBody] object relationships)
            => await base.PatchRelationshipAsync(id, relationshipName, relationships);

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(TId id) => await base.DeleteAsync(id);
    }
}
