using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCoreExample.Models;
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
                .Add<LegacyBlog>()
                .Add<Article>()
                .Add<Author>()
                .Add<Address>()
                .Add<Country>()
                .Add<Revision>()
                .Add<Tag>()
                .Build();

            Request = new JsonApiRequest
            {
                PrimaryResource = ResourceGraph.GetResourceContext<LegacyBlog>(),
                IsCollection = true
            };
        }
    }
}
