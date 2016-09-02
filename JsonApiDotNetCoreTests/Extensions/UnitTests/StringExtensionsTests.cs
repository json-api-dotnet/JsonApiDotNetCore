using Xunit;
using JsonApiDotNetCore.Extensions;

namespace JsonApiDotNetCoreTests.Extensions.UnitTests
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("TodoItem", "todoItem")]
        public void ToCamelCase_ConvertsString_ToCamelCase(string input, string expectedOutput)
        {
            // arrange
            // act
            var result = input.ToCamelCase();

            // assert
            Assert.Equal(expectedOutput, result);
        }

        [Theory]
        [InlineData("todoItem", "TodoItem")]
        [InlineData("todo-items", "TodoItems")]
        public void ToProperCase_ConvertsString_ToProperCase(string input, string expectedOutput)
        {
            // arrange
            // act
            var result = input.ToProperCase();

            // assert
            Assert.Equal(expectedOutput, result);
        }

        [Theory]
        [InlineData("todoItem", "todo-item")]
        [InlineData("TodoItem", "todo-item")]
        [InlineData("TodoItemS", "todo-item-s")]
        public void Dasherize_Converts_StringToDashed(string input, string expectedOutput)
        {
            // arrange
            // act
            var result = input.Dasherize();

            // assert
            Assert.Equal(expectedOutput, result);
        }
    }
}
