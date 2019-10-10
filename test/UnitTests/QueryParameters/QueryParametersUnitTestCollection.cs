using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using UnitTests.TestModels;

namespace UnitTests.QueryParameters
{
    public class QueryParametersUnitTestCollection
    {
        protected readonly ContextEntity _articleResourceContext;
        protected readonly IResourceGraph _graph;

        public QueryParametersUnitTestCollection()
        {
            var builder = new ResourceGraphBuilder();
            builder.AddResource<Article>();
            builder.AddResource<Person>();
            builder.AddResource<Blog>();
            builder.AddResource<Food>();
            builder.AddResource<Song>();
            _graph = builder.Build();
            _articleResourceContext = _graph.GetContextEntity<Article>();
        }
    }
}