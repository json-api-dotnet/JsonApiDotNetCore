using JsonApiDotNetCore.Resources.Annotations;
using Xunit;

namespace UnitTests.Models
{
    public sealed class AttributesEqualsTests
    {
        [Fact]
        public void HasManyAttribute_Equals_Returns_True_When_Same_Name()
        {
            var a = new HasManyAttribute
            {
                PublicName = "test"
            };

            var b = new HasManyAttribute
            {
                PublicName = "test"
            };

            Assert.Equal(a, b);
        }

        [Fact]
        public void HasManyAttribute_Equals_Returns_False_When_Different_Name()
        {
            var a = new HasManyAttribute
            {
                PublicName = "test"
            };

            var b = new HasManyAttribute
            {
                PublicName = "test2"
            };

            Assert.NotEqual(a, b);
        }

        [Fact]
        public void HasOneAttribute_Equals_Returns_True_When_Same_Name()
        {
            var a = new HasOneAttribute
            {
                PublicName = "test"
            };

            var b = new HasOneAttribute
            {
                PublicName = "test"
            };

            Assert.Equal(a, b);
        }

        [Fact]
        public void HasOneAttribute_Equals_Returns_False_When_Different_Name()
        {
            var a = new HasOneAttribute
            {
                PublicName = "test"
            };

            var b = new HasOneAttribute
            {
                PublicName = "test2"
            };

            Assert.NotEqual(a, b);
        }

        [Fact]
        public void AttrAttribute_Equals_Returns_True_When_Same_Name()
        {
            var a = new AttrAttribute
            {
                PublicName = "test"
            };

            var b = new AttrAttribute
            {
                PublicName = "test"
            };

            Assert.Equal(a, b);
        }

        [Fact]
        public void AttrAttribute_Equals_Returns_False_When_Different_Name()
        {
            var a = new AttrAttribute
            {
                PublicName = "test"
            };

            var b = new AttrAttribute
            {
                PublicName = "test2"
            };

            Assert.NotEqual(a, b);
        }

        [Fact]
        public void HasManyAttribute_Does_Not_Equal_HasOneAttribute_With_Same_Name()
        {
            RelationshipAttribute a = new HasManyAttribute
            {
                PublicName = "test"
            };

            RelationshipAttribute b = new HasOneAttribute
            {
                PublicName = "test"
            };

            Assert.NotEqual(a, b);
            Assert.NotEqual(b, a);
        }
    }
}
