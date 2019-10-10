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
            // arrange
            var e = Link.All;

            // assert
            Assert.True(e.HasFlag(Link.Self));
            Assert.True(e.HasFlag(Link.Paging));
            Assert.True(e.HasFlag(Link.Related));
            Assert.True(e.HasFlag(Link.All));
            Assert.False(e.HasFlag(Link.None));
        }

        [Fact]
        public void None_Contains_Only_None()
        {
            // arrange
            var e = Link.None;

            // assert
            Assert.False(e.HasFlag(Link.Self));
            Assert.False(e.HasFlag(Link.Paging));
            Assert.False(e.HasFlag(Link.Related));
            Assert.False(e.HasFlag(Link.All));
            Assert.True(e.HasFlag(Link.None));
        }

        [Fact]
        public void Self()
        {
            // arrange
            var e = Link.Self;

            // assert
            Assert.True(e.HasFlag(Link.Self));
            Assert.False(e.HasFlag(Link.Paging));
            Assert.False(e.HasFlag(Link.Related));
            Assert.False(e.HasFlag(Link.All));
            Assert.False(e.HasFlag(Link.None));
        }

        [Fact]
        public void Paging()
        {
            // arrange
            var e = Link.Paging;

            // assert
            Assert.False(e.HasFlag(Link.Self));
            Assert.True(e.HasFlag(Link.Paging));
            Assert.False(e.HasFlag(Link.Related));
            Assert.False(e.HasFlag(Link.All));
            Assert.False(e.HasFlag(Link.None));
        }

        [Fact]
        public void Related()
        {
            // arrange
            var e = Link.Related;

            // assert
            Assert.False(e.HasFlag(Link.Self));
            Assert.False(e.HasFlag(Link.Paging));
            Assert.True(e.HasFlag(Link.Related));
            Assert.False(e.HasFlag(Link.All));
            Assert.False(e.HasFlag(Link.None));
        }
    }
}
