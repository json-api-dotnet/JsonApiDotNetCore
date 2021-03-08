using System;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Resources;
using Xunit;

namespace UnitTests.Internal
{
    public sealed class TypeAnalysisTests
    {
        [Fact]
        public void New_Creates_An_Instance_If_T_Implements_Interface()
        {
            // Arrange
            Type type = typeof(Model);

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
            Type type = typeof(Model);

            // Act
            bool result = TypeHelper.IsOrImplementsInterface(type, typeof(IIdentifiable));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Implements_Returns_False_If_Type_DoesNot_Implement_Interface()
        {
            // Arrange
            Type type = typeof(string);

            // Act
            bool result = TypeHelper.IsOrImplementsInterface(type, typeof(IIdentifiable));

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
