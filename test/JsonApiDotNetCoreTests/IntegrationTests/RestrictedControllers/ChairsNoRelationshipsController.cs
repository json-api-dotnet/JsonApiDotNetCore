using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers
{
    public sealed class ChairsNoRelationshipsController : JsonApiController<Chair, int>
    {
        public ChairsNoRelationshipsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IGetAllService<Chair, int>? getAll, IGetByIdService<Chair, int>? getById, ICreateService<Chair, int>? create, IUpdateService<Chair, int>? update,
            IDeleteService<Chair, int>? delete)
            : base(options, resourceGraph, loggerFactory, getAll, getById, null, null, create, null, update, null, delete)
        {
        }
    }
}
