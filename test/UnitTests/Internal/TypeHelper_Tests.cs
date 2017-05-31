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
    }
}
