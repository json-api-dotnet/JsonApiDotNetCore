using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;
using JsonApiDotNetCore.Serialization;
using Moq;
using Xunit;
using UnitTests.TestModels;

namespace UnitTests.Serialization.Serializer
{
    public sealed class BaseDocumentBuilderTests : SerializerTestsSetup
    {
        private readonly TestDocumentBuilder _builder;

        public BaseDocumentBuilderTests()
        {
            var mock = new Mock<IResourceObjectBuilder>();
            mock.Setup(m => m.Build(It.IsAny<IIdentifiable>(), It.IsAny<IEnumerable<AttrAttribute>>(), It.IsAny<IEnumerable<RelationshipAttribute>>())).Returns(new ResourceObject());
            _builder = new TestDocumentBuilder(mock.Object);
        }


        [Fact]
        public void ResourceToDocument_NullResource_CanBuild()
        {
            // Act
            var document = _builder.Build((TestResource) null);

            // Assert
            Assert.Null(document.Data);
            Assert.False(document.IsPopulated);
        }


        [Fact]
        public void ResourceToDocument_EmptyList_CanBuild()
        {
            // Arrange
            var resources = new List<TestResource>();

            // Act
            var document = _builder.Build(resources);

            // Assert
            Assert.NotNull(document.Data);
            Assert.Empty(document.ManyData);
        }


        [Fact]
        public void ResourceToDocument_SingleResource_CanBuild()
        {
            // Arrange
            IIdentifiable dummy = new DummyResource();

            // Act
            var document = _builder.Build(dummy);

            // Assert
            Assert.NotNull(document.Data);
            Assert.True(document.IsPopulated);
        }

        [Fact]
        public void ResourceToDocument_ResourceList_CanBuild()
        {
            // Arrange
            var resources = new List<IIdentifiable> { new DummyResource(), new DummyResource() };

            // Act
            var document = _builder.Build(resources);
            var data = (List<ResourceObject>)document.Data;

            // Assert
            Assert.Equal(2, data.Count);
        }

        public sealed class DummyResource : Identifiable
        {
        }
    }
}
