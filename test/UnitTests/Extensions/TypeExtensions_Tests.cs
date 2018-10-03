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
            // arrange
            var type = typeof(Model);

            // act
            var collection = type.GetEmptyCollection();

            // assert
            Assert.NotNull(collection);
            Assert.Empty(collection);
            Assert.IsType<List<Model>>(collection);
        }

        [Fact]
        public void New_Creates_An_Instance_If_T_Implements_Interface()
        {
            // arrange
            var type = typeof(Model);

            // act
            var instance = type.New<IIdentifiable>();

            // assert
            Assert.NotNull(instance);
            Assert.IsType<Model>(instance);
        }

        [Fact]
        public void Implements_Returns_True_If_Type_Implements_Interface()
        {
            // arrange
            var type = typeof(Model);

            // act
            var result = type.Implements<IIdentifiable>();

            // assert
            Assert.True(result);
        }

        [Fact]
        public void Implements_Returns_False_If_Type_DoesNot_Implement_Interface()
        {
            // arrange
            var type = typeof(String);

            // act
            var result = type.Implements<IIdentifiable>();

            // assert
            Assert.False(result);
        }

        private class Model : IIdentifiable
        {
            public string StringId { get; set; }
        }
    }
}
