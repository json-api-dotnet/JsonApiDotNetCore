using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;
using Microsoft.Extensions.Logging.Abstractions;

namespace JsonApiDotNetCoreTests.UnitTests.QueryStringParameters;

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
            .Add<Blog, int>()
            .Add<BlogPost, int>()
            .Add<Label, int>()
            .Add<Comment, int>()
            .Add<WebAccount, int>()
            .Add<AccountPreferences, int>()
            .Add<LoginAttempt, int>()
            .Build();

        // @formatter:wrap_chained_method_calls restore
        // @formatter:keep_existing_linebreaks restore

        Request = new JsonApiRequest
        {
            PrimaryResourceType = ResourceGraph.GetResourceType<Blog>(),
            IsCollection = true
        };
    }
}
