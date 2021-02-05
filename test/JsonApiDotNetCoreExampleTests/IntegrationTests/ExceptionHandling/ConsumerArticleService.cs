using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ExceptionHandling
{
    public sealed class ConsumerArticleService : JsonApiResourceService<ConsumerArticle>
    {
        public const string UnavailableArticlePrefix = "X";

        private const string _supportEmailAddress = "company@email.com";

        public ConsumerArticleService(IResourceRepositoryAccessor repositoryAccessor, IQueryLayerComposer queryLayerComposer,
            IPaginationContext paginationContext, IJsonApiOptions options, ILoggerFactory loggerFactory,
            IJsonApiRequest request, IResourceChangeTracker<ConsumerArticle> resourceChangeTracker,
            IResourceHookExecutorFacade hookExecutor)
            : base(repositoryAccessor, queryLayerComposer, paginationContext, options, loggerFactory, request,
                resourceChangeTracker, hookExecutor)
        {
        }

        public override async Task<ConsumerArticle> GetAsync(int id, CancellationToken cancellationToken)
        {
            var consumerArticle = await base.GetAsync(id, cancellationToken);

            if (consumerArticle.Code.StartsWith(UnavailableArticlePrefix))
            {
                throw new ConsumerArticleIsNoLongerAvailableException(consumerArticle.Code, _supportEmailAddress);
            }

            return consumerArticle;
        }
    }
}
