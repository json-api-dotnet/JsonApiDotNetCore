using JsonApiDotNetCore.Resources.Annotations;
using Xunit;

namespace UnitTests.Models
{
    public sealed class AttributesEqualsTests
    {
        [Fact]
        public void HasManyAttribute_Equals_Returns_True_When_Same_Name()
        {
            var attribute1 = new HasManyAttribute
            {
                PublicName = "test"
            };

            var attribute2 = new HasManyAttribute
            {
                PublicName = "test"
            };

            Assert.Equal(attribute1, attribute2);
        }

        [Fact]
        public void HasManyAttribute_Equals_Returns_False_When_Different_Name()
        {
            var attribute1 = new HasManyAttribute
            {
                PublicName = "test"
            };

            var attribute2 = new HasManyAttribute
            {
                PublicName = "test2"
            };

            Assert.NotEqual(attribute1, attribute2);
        }

        [Fact]
        public void HasOneAttribute_Equals_Returns_True_When_Same_Name()
        {
            var attribute1 = new HasOneAttribute
            {
                PublicName = "test"
            };

            var attribute2 = new HasOneAttribute
            {
                PublicName = "test"
            };

            Assert.Equal(attribute1, attribute2);
        }

        [Fact]
        public void HasOneAttribute_Equals_Returns_False_When_Different_Name()
        {
            var attribute1 = new HasOneAttribute
            {
                PublicName = "test"
            };

            var attribute2 = new HasOneAttribute
            {
                PublicName = "test2"
            };

            Assert.NotEqual(attribute1, attribute2);
        }

        [Fact]
        public void AttrAttribute_Equals_Returns_True_When_Same_Name()
        {
            var attribute1 = new AttrAttribute
            {
                PublicName = "test"
            };

            var attribute2 = new AttrAttribute
            {
                PublicName = "test"
            };

            Assert.Equal(attribute1, attribute2);
        }

        [Fact]
        public void AttrAttribute_Equals_Returns_False_When_Different_Name()
        {
            var attribute1 = new AttrAttribute
            {
                PublicName = "test"
            };

            var attribute2 = new AttrAttribute
            {
                PublicName = "test2"
            };

            Assert.NotEqual(attribute1, attribute2);
        }

        [Fact]
        public void HasManyAttribute_Does_Not_Equal_HasOneAttribute_With_Same_Name()
        {
            RelationshipAttribute attribute1 = new HasManyAttribute
            {
                PublicName = "test"
            };

            RelationshipAttribute attribute2 = new HasOneAttribute
            {
                PublicName = "test"
            };

            Assert.NotEqual(attribute1, attribute2);
            Assert.NotEqual(attribute2, attribute1);
        }
    }
}
