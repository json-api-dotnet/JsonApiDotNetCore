using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
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
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class ConsumerArticleService : JsonApiResourceService<ConsumerArticle>
    {
        private const string SupportEmailAddress = "company@email.com";
        internal const string UnavailableArticlePrefix = "X";

        public ConsumerArticleService(IResourceRepositoryAccessor repositoryAccessor, IQueryLayerComposer queryLayerComposer,
            IPaginationContext paginationContext, IJsonApiOptions options, ILoggerFactory loggerFactory, IJsonApiRequest request,
            IResourceChangeTracker<ConsumerArticle> resourceChangeTracker, IResourceHookExecutorFacade hookExecutor)
            : base(repositoryAccessor, queryLayerComposer, paginationContext, options, loggerFactory, request, resourceChangeTracker, hookExecutor)
        {
        }

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
}
