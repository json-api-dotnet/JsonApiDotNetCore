using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests.Data
{
    public class EntityRepositoryTests
    {

        [Theory]
        [InlineData(6, -1, new[] { 4, 5, 6, 7, 8, 9 })]
        [InlineData(6, -2, new[] { 1, 2, 3 })]
        [InlineData(20, -1, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
        public async Task Paging_PageNumberIsNegative_PageCorrectIds(int pageSize, int pageNumber, int[] expectedIds)
        {
            // Arrange
            var todoItems = DbSetMock.Create(TodoItems(1, 2, 3, 4, 5, 6, 7, 8, 9)).Object;
            var repository = GetRepository();

            var result = await repository.PageAsync(todoItems, pageSize, pageNumber);

            Assert.Equal(TodoItems(expectedIds), result, new IdComparer<TodoItem>());
        }
    }
}
