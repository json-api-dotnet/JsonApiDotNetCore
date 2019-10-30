using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models;
using Xunit;

namespace UnitTests.Extensions
{
    public class TypeExtensions_Tests
    {
        [Fact]
        public void GetCollection_Creates_List_If_T_Implements_Interface()
        {
            // Arrange
            var type = typeof(Model);

            // Act
            var collection = type.GetEmptyCollection();

            // Assert
            Assert.NotNull(collection);
            Assert.Empty(collection);
            Assert.IsType<List<Model>>(collection);
        }

        [Fact]
        public void New_Creates_An_Instance_If_T_Implements_Interface()
        {
            // Arrange
            var type = typeof(Model);

            // Act
            var instance = type.New<IIdentifiable>();

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
            var result = type.Implements<IIdentifiable>();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Implements_Returns_False_If_Type_DoesNot_Implement_Interface()
        {
            // Arrange
            var type = typeof(String);

            // Act
            var result = type.Implements<IIdentifiable>();

            // Assert
            Assert.False(result);
        }

        private class Model : IIdentifiable
        {
            public string StringId { get; set; }
        }
    }
}
