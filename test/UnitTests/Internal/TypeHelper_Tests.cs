using System;
using System.Collections.Generic;
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

        [Fact]
        public void ConvertType_Returns_Default_Value_For_Empty_Strings()
        {
            // arrange -- can't use non-constants in [Theory]
            var data = new Dictionary<Type, object>
            {
                { typeof(int), 0 },
                { typeof(short), (short)0 },
                { typeof(long), (long)0 },
                { typeof(string), "" },
                { typeof(Guid), Guid.Empty },
            };

            foreach (var t in data)
            {
                // act
                var result = TypeHelper.ConvertType(string.Empty, t.Key);

                // assert
                Assert.Equal(t.Value, result);
            }
        }
        
        [Fact]
        public void Can_Convert_TimeSpans() 
        {
            //arrange
            TimeSpan timeSpan = TimeSpan.FromMinutes(45);
            string stringSpan = timeSpan.ToString();

            //act
            var result = TypeHelper.ConvertType(stringSpan, typeof(TimeSpan));

            //assert
            Assert.Equal(timeSpan, result);
        }

        [Fact]
        public void Bad_TimeSpanString_Throws() 
        {
            // arrange
            var formattedString = "this_is_not_a_valid_timespan";

            // act/assert
            Assert.Throws<FormatException>(() => TypeHelper.ConvertType(formattedString, typeof(TimeSpan)));
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
