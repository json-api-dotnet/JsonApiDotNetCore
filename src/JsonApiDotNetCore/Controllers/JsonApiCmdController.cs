using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Controllers
{
    public class JsonApiCmdController<T> : JsonApiCmdController<T, int>
        where T : class, IIdentifiable<int>
    {
        public JsonApiCmdController(
            IJsonApiOptions jsonApiOptions,
            IResourceService<T, int> resourceService)
            : base(jsonApiOptions, resourceService)
        { }
    }

    public class JsonApiCmdController<T, TId>
    : BaseJsonApiController<T, TId> where T : class, IIdentifiable<TId>
    {
        public JsonApiCmdController(
            IJsonApiOptions jsonApiOptions,
            IResourceService<T, TId> resourceService)
        : base(jsonApiOptions, resourceService)
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
