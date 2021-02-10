using System;
using System.Collections.Generic;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Resources;
using Xunit;

namespace UnitTests.Internal
{
    public sealed class TypeHelper_Tests
    {
        [Fact]
        public void Can_Convert_DateTimeOffsets()
        {
            // Arrange
            var dto = new DateTimeOffset(new DateTime(2002, 2,2), TimeSpan.FromHours(4));;
            var formattedString = dto.ToString("O");

            // Act
            var result = TypeHelper.ConvertType(formattedString, typeof(DateTimeOffset));

            // Assert
            Assert.Equal(dto, result);
        }

        [Fact]
        public void Bad_DateTimeOffset_String_Throws()
        {
            // Arrange
            var formattedString = "this_is_not_a_valid_dto";

            // Act
            // Assert
            Assert.Throws<FormatException>(() => TypeHelper.ConvertType(formattedString, typeof(DateTimeOffset)));
        }

        [Fact]
        public void Can_Convert_Enums()
        {
            // Arrange
            var formattedString = "1";

            // Act
            var result = TypeHelper.ConvertType(formattedString, typeof(TestEnum));

            // Assert
            Assert.Equal(TestEnum.Test, result);
        }

        [Fact]
        public void ConvertType_Returns_Value_If_Type_Is_Same()
        {
            // Arrange
            var val = new ComplexType
            {
                Property = 1
            };

            var type = val.GetType();

            // Act
            var result = TypeHelper.ConvertType(val, type);

            // Assert
            Assert.Equal(val, result);
        }

        [Fact]
        public void ConvertType_Returns_Value_If_Type_Is_Assignable()
        {
            // Arrange
            var val = new ComplexType
            {
                Property = 1
            };

            var baseType = typeof(BaseType);
            var iType = typeof(IType);

            // Act
            var baseResult = TypeHelper.ConvertType(val, baseType);
            var iResult = TypeHelper.ConvertType(val, iType);

            // Assert
            Assert.Equal(val, baseResult);
            Assert.Equal(val, iResult);
        }

        [Fact]
        public void ConvertType_Returns_Default_Value_For_Empty_Strings()
        {
            // Arrange -- can't use non-constants in [Theory]
            var data = new Dictionary<Type, object>
            {
                { typeof(int), 0 },
                { typeof(short), (short)0 },
                { typeof(long), (long)0 },
                { typeof(string), "" },
                { typeof(Guid), Guid.Empty }
            };

            foreach (var t in data)
            {
                // Act
                var result = TypeHelper.ConvertType(string.Empty, t.Key);

                // Assert
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
            // Arrange
            var formattedString = "this_is_not_a_valid_timespan";

            // Act/assert
            Assert.Throws<FormatException>(() => TypeHelper.ConvertType(formattedString, typeof(TimeSpan)));
        }

        [Fact]
        public void New_Creates_An_Instance_If_T_Implements_Interface()
        {
            // Arrange
            var type = typeof(Model);

            // Act
            var instance = (IIdentifiable)TypeHelper.CreateInstance(type);

            // Assert
            Assert.NotNull(instance);
            Assert.IsType<Model>(instance);
        }

        [Fact]
        public void Implements_Returns_True_If_Type_Implements_Interface()
        {
            // Arrange
            var type = typeof(Model);

            // Act
            var result = TypeHelper.IsOrImplementsInterface(type, typeof(IIdentifiable));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Implements_Returns_False_If_Type_DoesNot_Implement_Interface()
        {
            // Arrange
            var type = typeof(string);

            // Act
            var result = TypeHelper.IsOrImplementsInterface(type, typeof(IIdentifiable));

            // Assert
            Assert.False(result);
        }

        private enum TestEnum
        {
            Test = 1
        }

        private sealed class ComplexType : BaseType
        {
            public int Property { get; set; }
        }

        private class BaseType : IType
        { }

        private interface IType
        { }

        private sealed class Model : IIdentifiable
        {
            public string StringId { get; set; }
            public string LocalId { get; set; }
        }
    }
}
