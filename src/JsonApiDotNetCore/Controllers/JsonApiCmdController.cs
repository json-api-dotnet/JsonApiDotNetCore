using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Controllers
{
    public class JsonApiCmdController<T> : JsonApiCmdController<T, int> where T : class, IIdentifiable<int>
    {
        public JsonApiCmdController(IJsonApiOptions jsonApiOptions, ILoggerFactory loggerFactory,
            IResourceCmdService<T, int> cmdService)
            : base(jsonApiOptions, loggerFactory, cmdService)
        {
        }
    }

    public class JsonApiCmdController<T, TId> : BaseJsonApiController<T, TId> where T : class, IIdentifiable<TId>
    {
        public JsonApiCmdController(IJsonApiOptions jsonApiOptions, ILoggerFactory loggerFactory,
            IResourceCmdService<T, TId> cmdService)
            : base(jsonApiOptions, loggerFactory, null, cmdService)
        {
        }

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
