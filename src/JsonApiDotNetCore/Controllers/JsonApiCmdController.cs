using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Controllers
{
    public class JsonApiCmdController<T>
    : JsonApiCmdController<T, int> where T : class, IIdentifiable<int>
    {
        public JsonApiCmdController(
            IJsonApiContext jsonApiContext,
            IResourceService<T, int> resourceService)
            : base(jsonApiContext, resourceService)
        { }
    }

    public class JsonApiCmdController<T, TId>
    : BaseJsonApiController<T, TId> where T : class, IIdentifiable<TId>
    {
        public JsonApiCmdController(
            IJsonApiContext jsonApiContext,
            IResourceService<T, TId> resourceService)
        : base(jsonApiContext, resourceService)
        { }

        [HttpPost]
        public override async Task<IActionResult> PostAsync([FromBody] T entity)
            => await base.PostAsync(entity);

        [HttpPatch("{id}")]
        public override async Task<IActionResult> PatchAsync(TId id, [FromBody] T entity)
            => await base.PatchAsync(id, entity);

        [HttpPatch("{id}/relationships/{relationshipName}")]
        public override async Task<IActionResult> PatchRelationshipsAsync(
            TId id, string relationshipName, [FromBody] List<ResourceObject> relationships)
            => await base.PatchRelationshipsAsync(id, relationshipName, relationships);

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(TId id) => await base.DeleteAsync(id);
    }
}
