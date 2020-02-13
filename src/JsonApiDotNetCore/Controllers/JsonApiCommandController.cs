using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Controllers
{
    public class JsonApiCommandController<T> : JsonApiCommandController<T, int> where T : class, IIdentifiable<int>
    {
        public JsonApiCommandController(
            IJsonApiOptions jsonApiOptions,
            ILoggerFactory loggerFactory,
            IResourceCommandService<T, int> commandService)
            : base(jsonApiOptions, loggerFactory, commandService)
        { }
    }

    public class JsonApiCommandController<T, TId> : BaseJsonApiController<T, TId> where T : class, IIdentifiable<TId>
    {
        public JsonApiCommandController(
            IJsonApiOptions jsonApiOptions,
            ILoggerFactory loggerFactory,
            IResourceCommandService<T, TId> commandService)
            : base(jsonApiOptions, loggerFactory, null, commandService)
        { }

        [HttpPost]
        public override async Task<IActionResult> PostAsync([FromBody] T entity)
            => await base.PostAsync(entity);

        [HttpPatch("{id}")]
        public override async Task<IActionResult> PatchAsync(TId id, [FromBody] T entity)
            => await base.PatchAsync(id, entity);

        [HttpPatch("{id}/relationships/{relationshipName}")]
        public override async Task<IActionResult> PatchRelationshipsAsync(
            TId id, string relationshipName, [FromBody] object relationships)
            => await base.PatchRelationshipsAsync(id, relationshipName, relationships);

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(TId id) => await base.DeleteAsync(id);
    }
}
