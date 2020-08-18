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
            var e = Links.All;

            // Assert
            Assert.True(e.HasFlag(Links.Self));
            Assert.True(e.HasFlag(Links.Paging));
            Assert.True(e.HasFlag(Links.Related));
            Assert.True(e.HasFlag(Links.All));
            Assert.False(e.HasFlag(Links.None));
        }

        [Fact]
        public void None_Contains_Only_None()
        {
            // Arrange
            var e = Links.None;

            // Assert
            Assert.False(e.HasFlag(Links.Self));
            Assert.False(e.HasFlag(Links.Paging));
            Assert.False(e.HasFlag(Links.Related));
            Assert.False(e.HasFlag(Links.All));
            Assert.True(e.HasFlag(Links.None));
        }

        [Fact]
        public void Self()
        {
            // Arrange
            var e = Links.Self;

            // Assert
            Assert.True(e.HasFlag(Links.Self));
            Assert.False(e.HasFlag(Links.Paging));
            Assert.False(e.HasFlag(Links.Related));
            Assert.False(e.HasFlag(Links.All));
            Assert.False(e.HasFlag(Links.None));
        }

        [Fact]
        public void Paging()
        {
            // Arrange
            var e = Links.Paging;

            // Assert
            Assert.False(e.HasFlag(Links.Self));
            Assert.True(e.HasFlag(Links.Paging));
            Assert.False(e.HasFlag(Links.Related));
            Assert.False(e.HasFlag(Links.All));
            Assert.False(e.HasFlag(Links.None));
        }

        [Fact]
        public void Related()
        {
            // Arrange
            var e = Links.Related;

            // Assert
            Assert.False(e.HasFlag(Links.Self));
            Assert.False(e.HasFlag(Links.Paging));
            Assert.True(e.HasFlag(Links.Related));
            Assert.False(e.HasFlag(Links.All));
            Assert.False(e.HasFlag(Links.None));
        }
    }
}
