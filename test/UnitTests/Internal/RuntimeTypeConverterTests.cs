using System;
using System.Collections.Generic;
using FluentAssertions;
using JsonApiDotNetCore.Resources.Internal;
using Xunit;

namespace UnitTests.Internal
{
    public sealed class RuntimeTypeConverterTests
    {
        [Fact]
        public void Can_Convert_DateTimeOffsets()
        {
            // Arrange
            var dateTimeOffset = new DateTimeOffset(new DateTime(2002, 2, 2), TimeSpan.FromHours(4));
            string formattedString = dateTimeOffset.ToString("O");

            // Act
            object? result = RuntimeTypeConverter.ConvertType(formattedString, typeof(DateTimeOffset));

            // Assert
            result.Should().Be(dateTimeOffset);
        }

        [Fact]
        public void Bad_DateTimeOffset_String_Throws()
        {
            // Arrange
            const string formattedString = "this_is_not_a_valid_dto";

            // Act
            Action action = () => RuntimeTypeConverter.ConvertType(formattedString, typeof(DateTimeOffset));

            // Assert
            action.Should().ThrowExactly<FormatException>();
        }

        [Fact]
        public void Can_Convert_Enums()
        {
            // Arrange
            const string formattedString = "1";

            // Act
            object? result = RuntimeTypeConverter.ConvertType(formattedString, typeof(TestEnum));

            // Assert
            result.Should().Be(TestEnum.Test);
        }

        [Fact]
        public void ConvertType_Returns_Value_If_Type_Is_Same()
        {
            // Arrange
            var complexType = new ComplexType();
            Type type = complexType.GetType();

            // Act
            object? result = RuntimeTypeConverter.ConvertType(complexType, type);

            // Assert
            result.Should().Be(complexType);
        }

        [Fact]
        public void ConvertType_Returns_Value_If_Type_Is_Assignable()
        {
            // Arrange
            var complexType = new ComplexType();

            Type baseType = typeof(BaseType);
            Type iType = typeof(IType);

            // Act
            object? baseResult = RuntimeTypeConverter.ConvertType(complexType, baseType);
            object? iResult = RuntimeTypeConverter.ConvertType(complexType, iType);

            // Assert
            baseResult.Should().Be(complexType);
            iResult.Should().Be(complexType);
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

            foreach ((Type key, object value) in data)
            {
                // Act
                object? result = RuntimeTypeConverter.ConvertType(string.Empty, key);

                // Assert
                result.Should().Be(value);
            }
        }

        [Fact]
        public void Can_Convert_TimeSpans()
        {
            // Arrange
            TimeSpan timeSpan = TimeSpan.FromMinutes(45);
            string stringSpan = timeSpan.ToString();

            // Act
            object? result = RuntimeTypeConverter.ConvertType(stringSpan, typeof(TimeSpan));

            // Assert
            result.Should().Be(timeSpan);
        }

        [Fact]
        public void Bad_TimeSpanString_Throws()
        {
            // Arrange
            const string formattedString = "this_is_not_a_valid_timespan";

            // Act
            Action action = () => RuntimeTypeConverter.ConvertType(formattedString, typeof(TimeSpan));

            // Assert
            action.Should().ThrowExactly<FormatException>();
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
