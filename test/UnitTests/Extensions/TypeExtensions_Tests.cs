using JsonApiDotNetCore.Models;
using Xunit;
using JsonApiDotNetCore.Extensions;
using System.Collections.Generic;

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
            var collection = type.GetEmptyCollection<IIdentifiable>();

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

        private class Model : IIdentifiable
        {
            public string StringId { get; set; }
        }
    }
}
