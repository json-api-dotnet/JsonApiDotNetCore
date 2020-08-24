using JsonApiDotNetCore.Resources.Annotations;
using Xunit;

namespace UnitTests.Builders
{
    public sealed class LinkTests
    {
        [Theory]
        [InlineData(LinkTypes.All, LinkTypes.Self, true)]
        [InlineData(LinkTypes.All, LinkTypes.Related, true)]
        [InlineData(LinkTypes.All, LinkTypes.Paging, true)]
        [InlineData(LinkTypes.None, LinkTypes.Self, false)]
        [InlineData(LinkTypes.None, LinkTypes.Related, false)]
        [InlineData(LinkTypes.None, LinkTypes.Paging, false)]
        [InlineData(LinkTypes.NotConfigured, LinkTypes.Self, false)]
        [InlineData(LinkTypes.NotConfigured, LinkTypes.Related, false)]
        [InlineData(LinkTypes.NotConfigured, LinkTypes.Paging, false)]
        [InlineData(LinkTypes.Self, LinkTypes.Self, true)]
        [InlineData(LinkTypes.Self, LinkTypes.Related, false)]
        [InlineData(LinkTypes.Self, LinkTypes.Paging, false)]
        [InlineData(LinkTypes.Self, LinkTypes.None, false)]
        [InlineData(LinkTypes.Self, LinkTypes.NotConfigured, false)]
        [InlineData(LinkTypes.Related, LinkTypes.Self, false)]
        [InlineData(LinkTypes.Related, LinkTypes.Related, true)]
        [InlineData(LinkTypes.Related, LinkTypes.Paging, false)]
        [InlineData(LinkTypes.Related, LinkTypes.None, false)]
        [InlineData(LinkTypes.Related, LinkTypes.NotConfigured, false)]
        [InlineData(LinkTypes.Paging, LinkTypes.Self, false)]
        [InlineData(LinkTypes.Paging, LinkTypes.Related, false)]
        [InlineData(LinkTypes.Paging, LinkTypes.Paging, true)]
        [InlineData(LinkTypes.Paging, LinkTypes.None, false)]
        [InlineData(LinkTypes.Paging, LinkTypes.NotConfigured, false)]
        public void LinkHasFlag_BaseLinkAndCheckLink_ExpectedResult(LinkTypes baseLink, LinkTypes checkLink, bool equal)
        {
            Assert.Equal(equal, baseLink.HasFlag(checkLink));
        }
    }
}
