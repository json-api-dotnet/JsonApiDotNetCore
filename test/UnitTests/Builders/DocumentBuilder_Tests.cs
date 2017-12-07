using System;
using System.Collections;
using System.Collections.Generic;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Moq;
using Xunit;

namespace UnitTests
{
    public class DocumentBuilder_Tests
    {
        private readonly Mock<IJsonApiContext> _jsonApiContextMock;
        private readonly PageManager _pageManager;
        private readonly JsonApiOptions _options;

        public DocumentBuilder_Tests()
        {
            _jsonApiContextMock = new Mock<IJsonApiContext>();

            _options = new JsonApiOptions();

            _options.BuildContextGraph(builder =>
            {
                builder.AddResource<Model>("models");
            });

            _jsonApiContextMock
                .Setup(m => m.Options)
                .Returns(_options);

            _jsonApiContextMock
                .Setup(m => m.ContextGraph)
                .Returns(_options.ContextGraph);

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
                .Returns(_options.ContextGraph.GetContextEntity(typeof(Model)));
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

            _options.BuildContextGraph(builder => builder.DocumentLinks = Link.None);

            _jsonApiContextMock
                .Setup(m => m.ContextGraph)
                .Returns(_options.ContextGraph);

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
                .Setup(m => m.ContextGraph)
                .Returns(_options.ContextGraph);

            var documentBuilder = new DocumentBuilder(_jsonApiContextMock.Object);
            var entity = new Model();

            // act
            var document = documentBuilder.Build(entity);

            // assert
            Assert.Null(document.Data.Relationships["related-model"].Links);
        }

        [Fact]
        public void Build_Can_Build_Arrays()
        {
            var entities = new[] { new Model() };
            var documentBuilder = new DocumentBuilder(_jsonApiContextMock.Object);

            var documents = documentBuilder.Build(entities);

            Assert.Equal(1, documents.Data.Count);
        }

        [Fact]
        public void Build_Can_Build_CustomIEnumerables()
        {
            var entities = new Models(new[] { new Model() });
            var documentBuilder = new DocumentBuilder(_jsonApiContextMock.Object);

            var documents = documentBuilder.Build(entities);

            Assert.Equal(1, documents.Data.Count);
        }

        private class Model : Identifiable
        {
            [HasOne("related-model", Link.None)]
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
    }
}
