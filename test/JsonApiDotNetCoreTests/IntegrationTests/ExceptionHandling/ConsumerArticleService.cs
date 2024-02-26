using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.ExceptionHandling;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class ConsumerArticleService(
    IResourceRepositoryAccessor repositoryAccessor, IQueryLayerComposer queryLayerComposer, IPaginationContext paginationContext, IJsonApiOptions options,
    ILoggerFactory loggerFactory, IJsonApiRequest request, IResourceChangeTracker<ConsumerArticle> resourceChangeTracker,
    IResourceDefinitionAccessor resourceDefinitionAccessor) : JsonApiResourceService<ConsumerArticle, int>(repositoryAccessor, queryLayerComposer,
    paginationContext, options, loggerFactory, request, resourceChangeTracker, resourceDefinitionAccessor)
{
    private const string SupportEmailAddress = "company@email.com";
    internal const string UnavailableArticlePrefix = "X";

    public override async Task<ConsumerArticle> GetAsync(int id, CancellationToken cancellationToken)
    {
        ConsumerArticle consumerArticle = await base.GetAsync(id, cancellationToken);

        if (consumerArticle.Code.StartsWith(UnavailableArticlePrefix, StringComparison.Ordinal))
        {
            throw new ConsumerArticleIsNoLongerAvailableException(consumerArticle.Code, SupportEmailAddress);
        }

        return consumerArticle;
    }
}
