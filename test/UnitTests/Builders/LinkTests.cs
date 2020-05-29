using JsonApiDotNetCore.Models.JsonApiDocuments;
using Xunit;

namespace UnitTests.Builders
{
    public sealed class LinkTests
    {
        [Theory]
        [InlineData(Links.All, Links.Self, true)]
        [InlineData(Links.All, Links.Related, true)]
        [InlineData(Links.All, Links.Paging, true)]
        [InlineData(Links.None, Links.Self, false)]
        [InlineData(Links.None, Links.Related, false)]
        [InlineData(Links.None, Links.Paging, false)]
        [InlineData(Links.NotConfigured, Links.Self, false)]
        [InlineData(Links.NotConfigured, Links.Related, false)]
        [InlineData(Links.NotConfigured, Links.Paging, false)]
        [InlineData(Links.Self, Links.Self, true)]
        [InlineData(Links.Self, Links.Related, false)]
        [InlineData(Links.Self, Links.Paging, false)]
        [InlineData(Links.Self, Links.None, false)]
        [InlineData(Links.Self, Links.NotConfigured, false)]
        [InlineData(Links.Related, Links.Self, false)]
        [InlineData(Links.Related, Links.Related, true)]
        [InlineData(Links.Related, Links.Paging, false)]
        [InlineData(Links.Related, Links.None, false)]
        [InlineData(Links.Related, Links.NotConfigured, false)]
        [InlineData(Links.Paging, Links.Self, false)]
        [InlineData(Links.Paging, Links.Related, false)]
        [InlineData(Links.Paging, Links.Paging, true)]
        [InlineData(Links.Paging, Links.None, false)]
        [InlineData(Links.Paging, Links.NotConfigured, false)]
        public void LinkHasFlag_BaseLinkAndCheckLink_ExpectedResult(Links baseLink, Links checkLink, bool equal)
        {
            Assert.Equal(equal, baseLink.HasFlag(checkLink));
        }
    }
}
