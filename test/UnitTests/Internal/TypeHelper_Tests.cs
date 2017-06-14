using System;
using JsonApiDotNetCore.Internal;
using Xunit;

namespace UnitTests.Internal
{
    public class TypeHelper_Tests
    {
        [Fact]
        public void Can_Convert_DateTimeOffsets()
        {
            // arrange
            var dto = DateTimeOffset.Now;
            var formattedString = dto.ToString("O");

            // act
            var result = TypeHelper.ConvertType(formattedString, typeof(DateTimeOffset));

            // assert
            Assert.Equal(dto, result);
        }

        [Fact]
        public void Bad_DateTimeOffset_String_Throws()
        {
            // arrange
            var formattedString = "this_is_not_a_valid_dto";

            // act
            // assert
            Assert.Throws<FormatException>(() => TypeHelper.ConvertType(formattedString, typeof(DateTimeOffset)));
        }

        [Fact]
        public void Can_Convert_Enums()
        {
            // arrange
            var formattedString = "1";

            // act
            var result = TypeHelper.ConvertType(formattedString, typeof(TestEnum));

            // assert
            Assert.Equal(TestEnum.Test, result);
        }

        public enum TestEnum
        {
            Test = 1
        }
    }
}
