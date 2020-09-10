using JsonApiDotNetCore.Resources.Annotations;
using Xunit;

namespace UnitTests.Models
{
    public sealed class LinkTests
    {
        [Fact]
        public void All_Contains_All_Flags_Except_None()
        {
            // Arrange
            var e = LinkTypes.All;

            // Assert
            Assert.True(e.HasFlag(LinkTypes.Self));
            Assert.True(e.HasFlag(LinkTypes.Paging));
            Assert.True(e.HasFlag(LinkTypes.Related));
            Assert.True(e.HasFlag(LinkTypes.All));
            Assert.False(e.HasFlag(LinkTypes.None));
        }

        [Fact]
        public void None_Contains_Only_None()
        {
            // Arrange
            var e = LinkTypes.None;

            // Assert
            Assert.False(e.HasFlag(LinkTypes.Self));
            Assert.False(e.HasFlag(LinkTypes.Paging));
            Assert.False(e.HasFlag(LinkTypes.Related));
            Assert.False(e.HasFlag(LinkTypes.All));
            Assert.True(e.HasFlag(LinkTypes.None));
        }

        [Fact]
        public void Self()
        {
            // Arrange
            var e = LinkTypes.Self;

            // Assert
            Assert.True(e.HasFlag(LinkTypes.Self));
            Assert.False(e.HasFlag(LinkTypes.Paging));
            Assert.False(e.HasFlag(LinkTypes.Related));
            Assert.False(e.HasFlag(LinkTypes.All));
            Assert.False(e.HasFlag(LinkTypes.None));
        }

        [Fact]
        public void Paging()
        {
            // Arrange
            var e = LinkTypes.Paging;

            // Assert
            Assert.False(e.HasFlag(LinkTypes.Self));
            Assert.True(e.HasFlag(LinkTypes.Paging));
            Assert.False(e.HasFlag(LinkTypes.Related));
            Assert.False(e.HasFlag(LinkTypes.All));
            Assert.False(e.HasFlag(LinkTypes.None));
        }

        [Fact]
        public void Related()
        {
            // Arrange
            var e = LinkTypes.Related;

            // Assert
            Assert.False(e.HasFlag(LinkTypes.Self));
            Assert.False(e.HasFlag(LinkTypes.Paging));
            Assert.True(e.HasFlag(LinkTypes.Related));
            Assert.False(e.HasFlag(LinkTypes.All));
            Assert.False(e.HasFlag(LinkTypes.None));
        }
    }
}
