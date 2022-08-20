using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers;

public sealed partial class ChairsController : JsonApiController<Chair, int>
{
    public ChairsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IGetAllService<Chair, int> getAll,
        IGetByIdService<Chair, int> getById,
        ICreateService<Chair, int> create,
        IUpdateService<Chair, int> update,
        IDeleteService<Chair, int> delete)
        : base(options, resourceGraph, loggerFactory,
            getAll: getAll,
            getById: getById,
            create: create,
            update: update,
            delete: delete)
    {
    }
}
