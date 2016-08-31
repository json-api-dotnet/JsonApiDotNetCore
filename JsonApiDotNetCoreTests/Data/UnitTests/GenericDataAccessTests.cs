using JsonApiDotNetCoreTests.Data.TestData;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using JsonApiDotNetCore.Data;
using System.Linq;
using System.Collections.Generic;

namespace JsonApiDotNetCoreTests.Data.UnitTests
{
    public class GenericDataAccessTests
    {
        [Fact]
        public void SingleOrDefault_Fetches_SingleItemFromContext()
        {
            // arrange
            var data = new List<TodoItem>
            {
                new TodoItem { Id = 1, Name = "AAA" },
                new TodoItem { Id = 2, Name = "BBB" }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<TodoItem>>();
            mockSet.As<IQueryable<TodoItem>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<TodoItem>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<TodoItem>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<TodoItem>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var genericDataAccess = new GenericDataAccess();

            // act
            var item1 = genericDataAccess.SingleOrDefault<TodoItem>(mockSet.Object, "Id", 1);
            var item2 = genericDataAccess.SingleOrDefault<TodoItem>(mockSet.Object, "Name", "BBB");

            // assert
            Assert.Equal(1, item1.Id);
            Assert.Equal(2, item2.Id);
        }

        [Fact]
        public void Where_FetchesRecords_WherePropertyValueEquals_ProvidedValue()
        {
            // arrange
            var data = new List<TodoItem>
            {
                new TodoItem { Id = 1, Name = "AAA" },
                new TodoItem { Id = 2, Name = "AAA" },
                new TodoItem { Id = 3, Name = "BBB" }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<TodoItem>>();
            mockSet.As<IQueryable<TodoItem>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<TodoItem>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<TodoItem>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<TodoItem>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var genericDataAccess = new GenericDataAccess();

            // act
            var items = genericDataAccess.Where<TodoItem>(mockSet.Object, "Name", "AAA");

            // assert
            Assert.Equal(2, items.Count());
        }
    }
}
