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

            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            ResourceGraph = new ResourceGraphBuilder(Options, NullLoggerFactory.Instance)
                .Add<Blog>()
                .Add<BlogPost>()
                .Add<Label>()
                .Add<Comment>()
                .Add<WebAccount>()
                .Add<AccountPreferences>()
                .Build();

            // @formatter:wrap_chained_method_calls restore
            // @formatter:keep_existing_linebreaks restore

            Request = new JsonApiRequest
            {
                PrimaryResource = ResourceGraph.GetResourceContext<Blog>(),
                IsCollection = true
            };
        }
    }
}
