using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Building;
using JsonApiDotNetCore.Serialization.Objects;
using Moq;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Serialization.Serializer
{
    public sealed class BaseDocumentBuilderTests : SerializerTestsSetup
    {
        private readonly TestSerializer _builder;

        public BaseDocumentBuilderTests()
        {
            var mock = new Mock<IResourceObjectBuilder>();
            mock.Setup(m => m.Build(It.IsAny<IIdentifiable>(), It.IsAny<bool>(), It.IsAny<IReadOnlyCollection<AttrAttribute>>(), It.IsAny<IReadOnlyCollection<RelationshipAttribute>>())).Returns(new ResourceObject());
            _builder = new TestSerializer(mock.Object);
        }


        [Fact]
        public void ResourceToDocument_NullResource_CanBuild()
        {
            // Act
            var document = _builder.Build((TestResource) null, false);

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
            var document = _builder.Build(resources, false);

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
            var document = _builder.Build(dummy, false);

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
            var document = _builder.Build(resources, false);
            var data = (List<ResourceObject>)document.Data;

            // Assert
            Assert.Equal(2, data.Count);
        }

        public sealed class DummyResource : Identifiable
        {
        }
    }
}
