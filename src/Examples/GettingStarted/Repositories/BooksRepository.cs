using GettingStarted.Controllers;
using GettingStarted.Models;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;

namespace GettingStarted.Repositories;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class BooksRepository : EntityFrameworkCoreRepository<Book, int>
{
    private static readonly string BooksGetZipFileActionMethod = RemoveAsyncSuffix(nameof(BooksController.GetZipFileAsync));

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPaginationContext _paginationContext;

    private bool DisablePagination
    {
        get
        {
            RouteData routeData = _httpContextAccessor.HttpContext!.GetRouteData();
            return routeData.Values["action"]?.ToString() == BooksGetZipFileActionMethod;
        }
    }

    public BooksRepository(ITargetedFields targetedFields, IDbContextResolver dbContextResolver, IResourceGraph resourceGraph, IResourceFactory resourceFactory,
        IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory, IResourceDefinitionAccessor resourceDefinitionAccessor,
        IHttpContextAccessor httpContextAccessor, IPaginationContext paginationContext)
        : base(targetedFields, dbContextResolver, resourceGraph, resourceFactory, constraintProviders, loggerFactory, resourceDefinitionAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _paginationContext = paginationContext;
    }

    protected override IQueryable<Book> ApplyQueryLayer(QueryLayer queryLayer)
    {
        if (DisablePagination)
        {
            // Disable pagination when exporting to zip file.
            queryLayer.Pagination = null;
            _paginationContext.PageSize = null;
        }

        return base.ApplyQueryLayer(queryLayer);
    }

    private static string RemoveAsyncSuffix(string name)
    {
        const string asyncSuffix = "Async";
        return name.EndsWith(asyncSuffix, StringComparison.Ordinal) ? name[..^asyncSuffix.Length] : name;
    }
}
