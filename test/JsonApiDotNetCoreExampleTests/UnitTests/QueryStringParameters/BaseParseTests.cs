using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings;
using Microsoft.Extensions.Logging.Abstractions;

namespace JsonApiDotNetCoreExampleTests.UnitTests.QueryStringParameters
{
    public abstract class BaseParseTests
    {
        protected JsonApiOptions Options { get; }
        protected IResourceGraph ResourceGraph { get; }
        protected JsonApiRequest Request { get; }

        protected BaseParseTests()
        {
            Options = new JsonApiOptions();

            ResourceGraph = new ResourceGraphBuilder(Options, NullLoggerFactory.Instance)
                .Add<Blog>()
                .Add<BlogPost>()
                .Add<Label>()
                .Add<Comment>()
                .Add<WebAccount>()
                .Add<AccountPreferences>()
                .Build();

            Request = new JsonApiRequest
            {
                PrimaryResource = ResourceGraph.GetResourceContext<Blog>(),
                IsCollection = true
            };
        }
    }
}
