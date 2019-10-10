using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCore.Controllers
{
    public class JsonApiQueryController<T>
    : JsonApiQueryController<T, int> where T : class, IIdentifiable<int>
    {
        public JsonApiQueryController(
            IJsonApiOptions jsonApiOptions,
            IResourceService<T, int> resourceService)
            : base(jsonApiOptions, resourceService)
        { }
    }

    public class JsonApiQueryController<T, TId>
    : BaseJsonApiController<T, TId> where T : class, IIdentifiable<TId>
    {
        public JsonApiQueryController(
            IJsonApiOptions jsonApiOptions,
            IResourceService<T, TId> resourceService)
        : base(jsonApiOptions, resourceService)
        { }

        [HttpGet]
        public override async Task<IActionResult> GetAsync() => await base.GetAsync();

        [HttpGet("{id}")]
        public override async Task<IActionResult> GetAsync(TId id) => await base.GetAsync(id);

        [HttpGet("{id}/relationships/{relationshipName}")]
        public override async Task<IActionResult> GetRelationshipsAsync(TId id, string relationshipName)
            => await base.GetRelationshipsAsync(id, relationshipName);

        [HttpGet("{id}/{relationshipName}")]
        public override async Task<IActionResult> GetRelationshipAsync(TId id, string relationshipName)
            => await base.GetRelationshipAsync(id, relationshipName);
    }
}
