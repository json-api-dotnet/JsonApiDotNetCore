using System;
using System.Collections.Generic;
using JsonApiDotNetCore;
using Xunit;

namespace UnitTests.Internal
{
    public sealed class RuntimeTypeConverterTests
    {
        [Fact]
        public void Can_Convert_DateTimeOffsets()
        {
            // Arrange
            var dto = new DateTimeOffset(new DateTime(2002, 2, 2), TimeSpan.FromHours(4));
            string formattedString = dto.ToString("O");

            var typeConverter = new RuntimeTypeConverter();

            // Act
            object result = typeConverter.ConvertType(formattedString, typeof(DateTimeOffset));

            // Assert
            Assert.Equal(dto, result);
        }

        [Fact]
        public void Bad_DateTimeOffset_String_Throws()
        {
            // Arrange
            const string formattedString = "this_is_not_a_valid_dto";

            var typeConverter = new RuntimeTypeConverter();

            // Act
            Action action = () => typeConverter.ConvertType(formattedString, typeof(DateTimeOffset));

            // Assert
            Assert.Throws<FormatException>(action);
        }

        [Fact]
        public void Can_Convert_Enums()
        {
            // Arrange
            const string formattedString = "1";

            var typeConverter = new RuntimeTypeConverter();

            // Act
            object result = typeConverter.ConvertType(formattedString, typeof(TestEnum));

            // Assert
            Assert.Equal(TestEnum.Test, result);
        }

        [Fact]
        public void ConvertType_Returns_Value_If_Type_Is_Same()
        {
            // Arrange
            var val = new ComplexType();
            Type type = val.GetType();

            var typeConverter = new RuntimeTypeConverter();

            // Act
            object result = typeConverter.ConvertType(val, type);

            // Assert
            Assert.Equal(val, result);
        }

        [Fact]
        public void ConvertType_Returns_Value_If_Type_Is_Assignable()
        {
            // Arrange
            var val = new ComplexType();

            Type baseType = typeof(BaseType);
            Type iType = typeof(IType);

            var typeConverter = new RuntimeTypeConverter();

            // Act
            object baseResult = typeConverter.ConvertType(val, baseType);
            object iResult = typeConverter.ConvertType(val, iType);

            // Assert
            Assert.Equal(val, baseResult);
            Assert.Equal(val, iResult);
        }

        [Fact]
        public void ConvertType_Returns_Default_Value_For_Empty_Strings()
        {
            // Arrange
            var data = new Dictionary<Type, object>
            {
                { typeof(int), 0 },
                { typeof(short), (short)0 },
                { typeof(long), (long)0 },
                { typeof(string), "" },
                { typeof(Guid), Guid.Empty }
            };

            var typeConverter = new RuntimeTypeConverter();

            foreach (KeyValuePair<Type, object> pair in data)
            {
                // Act
                object result = typeConverter.ConvertType(string.Empty, pair.Key);

                // Assert
                Assert.Equal(pair.Value, result);
            }
        }

        [Fact]
        public void Can_Convert_TimeSpans()
        {
            // Arrange
            TimeSpan timeSpan = TimeSpan.FromMinutes(45);
            string stringSpan = timeSpan.ToString();

            var typeConverter = new RuntimeTypeConverter();

            // Act
            object result = typeConverter.ConvertType(stringSpan, typeof(TimeSpan));

            // Assert
            Assert.Equal(timeSpan, result);
        }

        [Fact]
        public void Bad_TimeSpanString_Throws()
        {
            // Arrange
            const string formattedString = "this_is_not_a_valid_timespan";

            var typeConverter = new RuntimeTypeConverter();

            // Act
            Action action = () => typeConverter.ConvertType(formattedString, typeof(TimeSpan));

            // Assert
            Assert.Throws<FormatException>(action);
        }

        private enum TestEnum
        {
            Test = 1
        }

        private sealed class ComplexType : BaseType
        {
        }

        private class BaseType : IType
        {
        }

        private interface IType
        {
        }
    }
}
