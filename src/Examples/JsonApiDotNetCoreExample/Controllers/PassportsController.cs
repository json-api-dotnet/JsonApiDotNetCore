using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public sealed class PassportsController : BaseJsonApiController<Passport>
    {
        public PassportsController(
            IJsonApiOptions jsonApiOptions,
            ILoggerFactory loggerFactory,
            IResourceService<Passport, int> resourceService)
            : base(jsonApiOptions, loggerFactory, resourceService)
        { }

        [HttpGet]
        public override async Task<IActionResult> GetAsync() => await base.GetAsync();

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(string id)
        {
            int idValue = HexadecimalObfuscationCodec.Decode(id);
            return await base.GetAsync(idValue);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchAsync(string id, [FromBody] Passport entity)
        {
            int idValue = HexadecimalObfuscationCodec.Decode(id);
            return await base.PatchAsync(idValue, entity);
        }

        [HttpPost]
        public override async Task<IActionResult> PostAsync([FromBody] Passport entity)
        {
            return await base.PostAsync(entity);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            int idValue = HexadecimalObfuscationCodec.Decode(id);
            return await base.DeleteAsync(idValue);
        }
    }
}
