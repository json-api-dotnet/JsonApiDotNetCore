using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using Xunit;

namespace UnitTests.Extensions
{
    public sealed class TypeExtensions_Tests
    {
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
            var result = type.IsOrImplementsInterface(typeof(IIdentifiable));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Implements_Returns_False_If_Type_DoesNot_Implement_Interface()
        {
            // Arrange
            var type = typeof(string);

            // Act
            var result = type.IsOrImplementsInterface(typeof(IIdentifiable));

            // Assert
            Assert.False(result);
        }

        private sealed class Model : IIdentifiable
        {
            public string StringId { get; set; }
        }
    }
}
