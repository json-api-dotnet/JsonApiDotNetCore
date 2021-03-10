using System;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Resources;
using Xunit;

namespace UnitTests.Internal
{
    public sealed class TypeExtensionsTests
    {
        [Fact]
        public void Implements_Returns_True_If_Type_Implements_Interface()
        {
            // Arrange
            Type type = typeof(Model);

            // Act
            bool result = type.IsOrImplementsInterface(typeof(IIdentifiable));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Implements_Returns_False_If_Type_DoesNot_Implement_Interface()
        {
            // Arrange
            Type type = typeof(string);

            // Act
            bool result = type.IsOrImplementsInterface(typeof(IIdentifiable));

            // Assert
            Assert.False(result);
        }

        private sealed class Model : IIdentifiable
        {
            public string StringId { get; set; }
            public string LocalId { get; set; }
        }
    }
}
