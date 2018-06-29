using JsonApiDotNetCore.Models;
using Xunit;

namespace UnitTests.Models
{
    public class IdentifiableTests
    {
        [Fact]
        public void Can_Set_StringId_To_Value_Type()
        {
            var resource = new IntId();
            resource.StringId = "1";
            Assert.Equal(1, resource.Id);
        }

        [Fact]
        public void Setting_StringId_To_Null_Sets_Id_As_Default()
        {
            var resource = new IntId();
            resource.StringId = null;
            Assert.Equal(0, resource.Id);
        }

        private class IntId : Identifiable { }
    }
}
