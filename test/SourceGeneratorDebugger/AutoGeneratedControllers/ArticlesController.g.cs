using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using SourceGeneratorDebugger.Models;

namespace SourceGeneratorDebugger.Controllers;

public sealed partial class ArticlesController : JsonApiController<Article, System.Guid>
{
    public ArticlesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IGetAllService<Article, System.Guid> getAll,
        IGetByIdService<Article, System.Guid> getById,
        IGetSecondaryService<Article, System.Guid> getSecondary)
        : base(options, resourceGraph, loggerFactory,
            getAll: getAll,
            getById: getById,
            getSecondary: getSecondary)
    {
    }
}
