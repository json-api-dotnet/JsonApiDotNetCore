using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTests.QueryStringParameters
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
                .AddResource<Blog>()
                .AddResource<Article>()
                .AddResource<Author>()
                .AddResource<Address>()
                .AddResource<Country>()
                .AddResource<Revision>()
                .AddResource<Tag>()
                .Build();

            Request = new JsonApiRequest
            {
                PrimaryResource = ResourceGraph.GetResourceContext<Blog>(),
                IsCollection = true
            };
        }
    }
}
