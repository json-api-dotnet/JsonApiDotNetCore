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

        [Fact]
        public void ConvertType_Returns_Value_If_Type_Is_Same()
        {
            // arrange
            var val = new ComplexType
            {
                Property = 1
            };

            var type = val.GetType();

            // act
            var result = TypeHelper.ConvertType(val, type);

            // assert
            Assert.Equal(val, result);
        }

        [Fact]
        public void ConvertType_Returns_Value_If_Type_Is_Assignable()
        {
            // arrange
            var val = new ComplexType
            {
                Property = 1
            };

            var baseType = typeof(BaseType);
            var iType = typeof(IType);

            // act
            var baseResult = TypeHelper.ConvertType(val, baseType);
            var iResult = TypeHelper.ConvertType(val, iType);

            // assert
            Assert.Equal(val, baseResult);
            Assert.Equal(val, iResult);
        }

        private enum TestEnum
        {
            Test = 1
        }

        private class ComplexType : BaseType
        {
            public int Property { get; set; }
        }

        private class BaseType : IType
        { }

        private interface IType
        { }
    }
}
