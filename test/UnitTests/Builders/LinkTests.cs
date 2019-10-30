using JsonApiDotNetCore.Models.Links;
using Xunit;

namespace UnitTests.Builders
{
    public class LinkTests
    {
        [Theory]
        [InlineData(Link.All, Link.Self, true)]
        [InlineData(Link.All, Link.Related, true)]
        [InlineData(Link.All, Link.Paging, true)]
        [InlineData(Link.None, Link.Self, false)]
        [InlineData(Link.None, Link.Related, false)]
        [InlineData(Link.None, Link.Paging, false)]
        [InlineData(Link.NotConfigured, Link.Self, false)]
        [InlineData(Link.NotConfigured, Link.Related, false)]
        [InlineData(Link.NotConfigured, Link.Paging, false)]
        [InlineData(Link.Self, Link.Self, true)]
        [InlineData(Link.Self, Link.Related, false)]
        [InlineData(Link.Self, Link.Paging, false)]
        [InlineData(Link.Self, Link.None, false)]
        [InlineData(Link.Self, Link.NotConfigured, false)]
        [InlineData(Link.Related, Link.Self, false)]
        [InlineData(Link.Related, Link.Related, true)]
        [InlineData(Link.Related, Link.Paging, false)]
        [InlineData(Link.Related, Link.None, false)]
        [InlineData(Link.Related, Link.NotConfigured, false)]
        [InlineData(Link.Paging, Link.Self, false)]
        [InlineData(Link.Paging, Link.Related, false)]
        [InlineData(Link.Paging, Link.Paging, true)]
        [InlineData(Link.Paging, Link.None, false)]
        [InlineData(Link.Paging, Link.NotConfigured, false)]
        public void LinkHasFlag_BaseLinkAndCheckLink_ExpectedResult(Link baseLink, Link checkLink, bool equal)
        {
            Assert.Equal(equal, baseLink.HasFlag(checkLink));
        }
    }
}
