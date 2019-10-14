using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Managers.Contracts;
using Moq;
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

        public ICurrentRequest CurrentRequestMockFactory(ContextEntity requestResource = null)
        {
            var mock = new Mock<ICurrentRequest>();

            if (requestResource != null)
                mock.Setup(m => m.GetRequestResource()).Returns(requestResource);

            return mock.Object;
        }
    }
}