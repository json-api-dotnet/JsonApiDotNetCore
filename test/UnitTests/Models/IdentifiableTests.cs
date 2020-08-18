using JsonApiDotNetCore.Resources;
using Xunit;

namespace UnitTests.Models
{
    public sealed class IdentifiableTests
    {
        [Fact]
        public void Can_Set_StringId_To_Value_Type()
        {
            var resource = new IntId {StringId = "1"};
            Assert.Equal(1, resource.Id);
        }

        [Fact]
        public void Setting_StringId_To_Null_Sets_Id_As_Default()
        {
            var resource = new IntId {StringId = null};
            Assert.Equal(0, resource.Id);
        }

        [Fact]
        public void GetStringId_Returns_EmptyString_If_Object_Is_Null()
        {
            var resource = new IntId();
            var stringId = resource.ExposedGetStringId(null);
            Assert.Equal(string.Empty, stringId);
        }

        private sealed class IntId : Identifiable { 
            public string ExposedGetStringId(object value) => GetStringId(value);
        }
    }
}
