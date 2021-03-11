using System.Collections.Generic;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Building;
using JsonApiDotNetCore.Serialization.Objects;
using Moq;
using UnitTests.TestModels;
using Xunit;

namespace UnitTests.Serialization.Common
{
    public sealed class BaseDocumentBuilderTests : SerializerTestsSetup
    {
        private readonly TestSerializer _builder;

        public BaseDocumentBuilderTests()
        {
            var mock = new Mock<IResourceObjectBuilder>();

            mock.Setup(builder => builder.Build(It.IsAny<IIdentifiable>(), It.IsAny<IReadOnlyCollection<AttrAttribute>>(),
                It.IsAny<IReadOnlyCollection<RelationshipAttribute>>())).Returns(new ResourceObject());

            _builder = new TestSerializer(mock.Object);
        }

        [Fact]
        public void ResourceToDocument_NullResource_CanBuild()
        {
            // Act
            Document document = _builder.PublicBuild((TestResource)null);

            // Assert
            Assert.Null(document.Data);
            Assert.False(document.IsPopulated);
        }

        [Fact]
        public void ResourceToDocument_EmptyList_CanBuild()
        {
            // Act
            Document document = _builder.PublicBuild(new List<TestResource>());

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
            Document document = _builder.PublicBuild(dummy);

            // Assert
            Assert.NotNull(document.Data);
            Assert.True(document.IsPopulated);
        }

        [Fact]
        public void ResourceToDocument_ResourceList_CanBuild()
        {
            // Arrange
            DummyResource[] resources = ArrayFactory.Create(new DummyResource(), new DummyResource());

            // Act
            Document document = _builder.PublicBuild(resources);
            var data = (List<ResourceObject>)document.Data;

            // Assert
            Assert.Equal(2, data.Count);
        }

        private sealed class DummyResource : Identifiable
        {
        }
    }
}
