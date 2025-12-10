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
        // @formatter:wrap_before_first_method_call true

        ResourceGraph = new ResourceGraphBuilder(Options, NullLoggerFactory.Instance)
            .Add<Blog, long>()
            .Add<BlogPost, long>()
            .Add<Label, long>()
            .Add<Comment, long>()
            .Add<WebAccount, long>()
            .Add<Human, long>()
            .Add<Man, long>()
            .Add<Woman, long>()
            .Add<AccountPreferences, long>()
            .Add<LoginAttempt, long>()
            .Build();

        // @formatter:wrap_chained_method_calls restore
        // @formatter:wrap_before_first_method_call restore

        Request = new JsonApiRequest
        {
            PrimaryResourceType = ResourceGraph.GetResourceType<Blog>(),
            IsCollection = true
        };
    }
}
