using System.Collections;
using System.Collections.Generic;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace UnitTests
{
    public class DocumentBuilder_Tests
    {
        private readonly Mock<IJsonApiContext> _jsonApiContextMock;
        private readonly PageManager _pageManager;
        private readonly JsonApiOptions _options;
        private readonly Mock<IRequestMeta> _requestMetaMock;

        public DocumentBuilder_Tests()
        {
            _jsonApiContextMock = new Mock<IJsonApiContext>();
            _requestMetaMock = new Mock<IRequestMeta>();

            _options = new JsonApiOptions();

            _options.BuildResourceGraph(builder =>
            {
                builder.AddResource<Model>("models");
                builder.AddResource<RelatedModel>("related-models");
            });

            _jsonApiContextMock
                .Setup(m => m.Options)
                .Returns(_options);

            _jsonApiContextMock
                .Setup(m => m.ResourceGraph)
                .Returns(_options.ResourceGraph);

            _jsonApiContextMock
                .Setup(m => m.MetaBuilder)
                .Returns(new MetaBuilder());

            _pageManager = new PageManager();
            _jsonApiContextMock
                .Setup(m => m.PageManager)
                .Returns(_pageManager);

            _jsonApiContextMock
                .Setup(m => m.BasePath)
                .Returns("localhost");

            _jsonApiContextMock
                .Setup(m => m.RequestEntity)
                .Returns(_options.ResourceGraph.GetContextEntity(typeof(Model)));
        }

        [Fact]
        public void Includes_Paging_Links_By_Default()
        {
            // arrange
            _pageManager.PageSize = 1;
            _pageManager.TotalRecords = 1;
            _pageManager.CurrentPage = 1;

            var documentBuilder = new DocumentBuilder(_jsonApiContextMock.Object);
            var entity = new Model();

            // act
            var document = documentBuilder.Build(entity);

            // assert
            Assert.NotNull(document.Links);
            Assert.NotNull(document.Links.Last);
        }

        [Fact]
        public void Page_Links_Can_Be_Disabled_Globally()
        {
            // arrange
            _pageManager.PageSize = 1;
            _pageManager.TotalRecords = 1;
            _pageManager.CurrentPage = 1;

            _options.BuildResourceGraph(builder => builder.DocumentLinks = Link.None);

            _jsonApiContextMock
                .Setup(m => m.ResourceGraph)
                .Returns(_options.ResourceGraph);

            var documentBuilder = new DocumentBuilder(_jsonApiContextMock.Object);
            var entity = new Model();

            // act
            var document = documentBuilder.Build(entity);

            // assert
            Assert.Null(document.Links);
        }

        [Fact]
        public void Related_Links_Can_Be_Disabled()
        {
            // arrange
            _pageManager.PageSize = 1;
            _pageManager.TotalRecords = 1;
            _pageManager.CurrentPage = 1;

            _jsonApiContextMock
                .Setup(m => m.ResourceGraph)
                .Returns(_options.ResourceGraph);

            var documentBuilder = new DocumentBuilder(_jsonApiContextMock.Object);
            var entity = new Model();

            // act
            var document = documentBuilder.Build(entity);

            // assert
            Assert.Null(document.Data.Relationships["related-model"].Links);
        }

        [Fact]
        public void Related_Links_Can_Be_Disabled_Globally()
        {
            // arrange
            _pageManager.PageSize = 1;
            _pageManager.TotalRecords = 1;
            _pageManager.CurrentPage = 1;

            _options.DefaultRelationshipLinks = Link.None;

            _jsonApiContextMock
                .Setup(m => m.ResourceGraph)
                .Returns(_options.ResourceGraph);

            var documentBuilder = new DocumentBuilder(_jsonApiContextMock.Object);
            var entity = new RelatedModel();

            // act
            var document = documentBuilder.Build(entity);

            // assert
            Assert.Null(document.Data.Relationships["models"].Links);
        }

        [Fact]
        public void Related_Data_Included_In_Relationships_By_Default()
        {
            // arrange
            const string relatedTypeName = "related-models";
            const string relationshipName = "related-model";
            const int relatedId = 1;
            _jsonApiContextMock
                .Setup(m => m.ResourceGraph)
                .Returns(_options.ResourceGraph);

            var documentBuilder = new DocumentBuilder(_jsonApiContextMock.Object);
            var entity = new Model
            {
                RelatedModel = new RelatedModel
                {
                    Id = relatedId
                }
            };

            // act
            var document = documentBuilder.Build(entity);

            // assert
            var relationshipData = document.Data.Relationships[relationshipName];
            Assert.NotNull(relationshipData);
            Assert.NotNull(relationshipData.SingleData);
            Assert.NotNull(relationshipData.SingleData);
            Assert.Equal(relatedId.ToString(), relationshipData.SingleData.Id);
            Assert.Equal(relatedTypeName, relationshipData.SingleData.Type);
        }

        [Fact]
        public void IndependentIdentifier_Included_In_HasOne_Relationships_By_Default()
        {
            // arrange
            const string relatedTypeName = "related-models";
            const string relationshipName = "related-model";
            const int relatedId = 1;
            _jsonApiContextMock
                .Setup(m => m.ResourceGraph)
                .Returns(_options.ResourceGraph);

            var documentBuilder = new DocumentBuilder(_jsonApiContextMock.Object);
            var entity = new Model
            {
                RelatedModelId = relatedId
            };

            // act
            var document = documentBuilder.Build(entity);

            // assert
            var relationshipData = document.Data.Relationships[relationshipName];
            Assert.NotNull(relationshipData);
            Assert.NotNull(relationshipData.SingleData);
            Assert.NotNull(relationshipData.SingleData);
            Assert.Equal(relatedId.ToString(), relationshipData.SingleData.Id);
            Assert.Equal(relatedTypeName, relationshipData.SingleData.Type);
        }

        [Fact]
        public void Build_Can_Build_Arrays()
        {
            var entities = new[] { new Model() };
            var documentBuilder = new DocumentBuilder(_jsonApiContextMock.Object);

            var documents = documentBuilder.Build(entities);

            Assert.Single(documents.Data);
        }

        [Fact]
        public void Build_Can_Build_CustomIEnumerables()
        {
            var entities = new Models(new[] { new Model() });
            var documentBuilder = new DocumentBuilder(_jsonApiContextMock.Object);

            var documents = documentBuilder.Build(entities);

            Assert.Single(documents.Data);
        }

        [Theory]
        [InlineData(null, null, true)]
        [InlineData(false, null, true)]
        [InlineData(true, null, false)]
        [InlineData(null, "foo", true)]
        [InlineData(false, "foo", true)]
        [InlineData(true, "foo", true)]
        public void DocumentBuilderOptions(
            bool? omitNullValuedAttributes,
            string attributeValue,
            bool resultContainsAttribute)
        {
            var documentBuilderBehaviourMock = new Mock<IDocumentBuilderOptionsProvider>();
            if (omitNullValuedAttributes.HasValue)
            {
                documentBuilderBehaviourMock.Setup(m => m.GetDocumentBuilderOptions())
                    .Returns(new DocumentBuilderOptions(omitNullValuedAttributes.Value));
            }
            var documentBuilder = new DocumentBuilder(_jsonApiContextMock.Object, null, omitNullValuedAttributes.HasValue ? documentBuilderBehaviourMock.Object : null);
            var document = documentBuilder.Build(new Model() { StringProperty = attributeValue });

            Assert.Equal(resultContainsAttribute, document.Data.Attributes.ContainsKey("StringProperty"));
        }

        private class Model : Identifiable
        {
            [Attr("StringProperty")] public string StringProperty { get; set; }

            [HasOne("related-model", documentLinks: Link.None)]
            public RelatedModel RelatedModel { get; set; }
            public int RelatedModelId { get; set; }
        }

        private class RelatedModel : Identifiable
        {
            [HasMany("models")]
            public List<Model> Models { get; set; }
        }

        private class Models : IEnumerable<Model>
        {
            private readonly IEnumerable<Model> models;

            public Models(IEnumerable<Model> models)
            {
                this.models = models;
            }

            public IEnumerator<Model> GetEnumerator()
            {
                return models.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return models.GetEnumerator();
            }
        }

        [Fact]
        public void Build_Will_Use_Resource_If_Defined_For_Multiple_Documents()
        {
            var entities = new[] { new User() };
            var resourceGraph = new ResourceGraphBuilder()
                    .AddResource<User>("user")
                    .Build();
            _jsonApiContextMock.Setup(m => m.ResourceGraph).Returns(resourceGraph);

            var scopedServiceProvider = new TestScopedServiceProvider(
                new ServiceCollection()
                    .AddScoped<ResourceDefinition<User>, UserResource>()
                    .BuildServiceProvider());

            var documentBuilder = new DocumentBuilder(_jsonApiContextMock.Object, scopedServiceProvider: scopedServiceProvider);

            var documents = documentBuilder.Build(entities);

            Assert.Single(documents.Data);
            Assert.False(documents.Data[0].Attributes.ContainsKey("password"));
            Assert.True(documents.Data[0].Attributes.ContainsKey("username"));
        }

        [Fact]
        public void Build_Will_Use_Resource_If_Defined_For_Single_Document()
        {
            var entity = new User();
            var resourceGraph = new ResourceGraphBuilder()
                    .AddResource<User>("user")
                    .Build();
            _jsonApiContextMock.Setup(m => m.ResourceGraph).Returns(resourceGraph);

            var scopedServiceProvider = new TestScopedServiceProvider(
                new ServiceCollection()
                    .AddScoped<ResourceDefinition<User>, UserResource>()
                    .BuildServiceProvider());

            var documentBuilder = new DocumentBuilder(_jsonApiContextMock.Object, scopedServiceProvider: scopedServiceProvider);

            var documents = documentBuilder.Build(entity);

            Assert.False(documents.Data.Attributes.ContainsKey("password"));
            Assert.True(documents.Data.Attributes.ContainsKey("username"));
        }

        [Fact]
        public void Build_Will_Use_Instance_Specific_Resource_If_Defined_For_Multiple_Documents()
        {
            var entities = new[] { new User() };
            var resourceGraph = new ResourceGraphBuilder()
                    .AddResource<User>("user")
                    .Build();
            _jsonApiContextMock.Setup(m => m.ResourceGraph).Returns(resourceGraph);

            var scopedServiceProvider = new TestScopedServiceProvider(
                new ServiceCollection()
                    .AddScoped<ResourceDefinition<User>, InstanceSpecificUserResource>()
                    .BuildServiceProvider());

            var documentBuilder = new DocumentBuilder(_jsonApiContextMock.Object, scopedServiceProvider: scopedServiceProvider);

            var documents = documentBuilder.Build(entities);

            Assert.Single(documents.Data);
            Assert.False(documents.Data[0].Attributes.ContainsKey("password"));
            Assert.True(documents.Data[0].Attributes.ContainsKey("username"));
        }

        [Fact]
        public void Build_Will_Use_Instance_Specific_Resource_If_Defined_For_Single_Document()
        {
            var entity = new User();
            var resourceGraph = new ResourceGraphBuilder()
                    .AddResource<User>("user")
                    .Build();
            _jsonApiContextMock.Setup(m => m.ResourceGraph).Returns(resourceGraph);

            var scopedServiceProvider = new TestScopedServiceProvider(
                new ServiceCollection()
                    .AddScoped<ResourceDefinition<User>, InstanceSpecificUserResource>()
                    .BuildServiceProvider());

            var documentBuilder = new DocumentBuilder(_jsonApiContextMock.Object, scopedServiceProvider: scopedServiceProvider);

            var documents = documentBuilder.Build(entity);
            
            Assert.False(documents.Data.Attributes.ContainsKey("password"));
            Assert.True(documents.Data.Attributes.ContainsKey("username"));
        }

        public class User : Identifiable
        {
            [Attr("username")] public string Username { get; set; }
            [Attr("password")] public string Password { get; set; }
        }

        public class InstanceSpecificUserResource : ResourceDefinition<User>
        {
            protected override List<AttrAttribute> OutputAttrs(User instance)
                => Remove(user => user.Password);
        }

        public class UserResource : ResourceDefinition<User>
        {
            protected override List<AttrAttribute> OutputAttrs()
                => Remove(user => user.Password);
        }
    }
}
