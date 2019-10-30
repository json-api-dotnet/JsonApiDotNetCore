using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Links;
using Xunit;

namespace UnitTests.Models
{
    public class LinkTests
    {
        [Fact]
        public void All_Contains_All_Flags_Except_None()
        {
            // Arrange
            var e = Link.All;

            // Assert
            Assert.True(e.HasFlag(Link.Self));
            Assert.True(e.HasFlag(Link.Paging));
            Assert.True(e.HasFlag(Link.Related));
            Assert.True(e.HasFlag(Link.All));
            Assert.False(e.HasFlag(Link.None));
        }

        [Fact]
        public void None_Contains_Only_None()
        {
            // Arrange
            var e = Link.None;

            // Assert
            Assert.False(e.HasFlag(Link.Self));
            Assert.False(e.HasFlag(Link.Paging));
            Assert.False(e.HasFlag(Link.Related));
            Assert.False(e.HasFlag(Link.All));
            Assert.True(e.HasFlag(Link.None));
        }

        [Fact]
        public void Self()
        {
            // Arrange
            var e = Link.Self;

            // Assert
            Assert.True(e.HasFlag(Link.Self));
            Assert.False(e.HasFlag(Link.Paging));
            Assert.False(e.HasFlag(Link.Related));
            Assert.False(e.HasFlag(Link.All));
            Assert.False(e.HasFlag(Link.None));
        }

        [Fact]
        public void Paging()
        {
            // Arrange
            var e = Link.Paging;

            // Assert
            Assert.False(e.HasFlag(Link.Self));
            Assert.True(e.HasFlag(Link.Paging));
            Assert.False(e.HasFlag(Link.Related));
            Assert.False(e.HasFlag(Link.All));
            Assert.False(e.HasFlag(Link.None));
        }

        [Fact]
        public void Related()
        {
            // Arrange
            var e = Link.Related;

            // Assert
            Assert.False(e.HasFlag(Link.Self));
            Assert.False(e.HasFlag(Link.Paging));
            Assert.True(e.HasFlag(Link.Related));
            Assert.False(e.HasFlag(Link.All));
            Assert.False(e.HasFlag(Link.None));
        }
    }
}
