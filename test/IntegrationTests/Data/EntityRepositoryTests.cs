using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCoreExample.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace JADNC.IntegrationTests.Data
{
    public class EntityRepositoryTests
    {


        public EntityRepositoryTests()
        {
            // setup database + services
            // seed
        }
        [Theory]
        [InlineData(6, -1, new[] { 4, 5, 6, 7, 8, 9 })]
        [InlineData(6, -2, new[] { 1, 2, 3 })]
        [InlineData(20, -1, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
        public async Task Paging_PageNumberIsNegative_PageCorrectIds(int pageSize, int pageNumber, int[] expectedIds)
        {
            // Arrange
            var todoItems = DbSetMock.Create(TodoItems(1, 2, 3, 4, 5, 6, 7, 8, 9)).Object;
            var repository = GetRepository();

            // Act
            var result = await repository.PageAsync(todoItems, pageSize, pageNumber);

            // Assert
            Assert.Equal(TodoItems(expectedIds), result, new IdComparer<TodoItem>());
        }


        private DefaultResourceRepository<TodoItem> GetRepository()
        {

            var contextResolverMock = new Mock<IDbContextResolver>();
            _contextMock
                .Setup(m => m.Set<TodoItem>())
                .Returns(_dbSetMock.Object);

            contextResolverMock
                .Setup(m => m.GetContext())
                .Returns(_contextMock.Object);

            var resourceGraph = new ResourceGraphBuilder().AddResource<TodoItem>().Build();


            return new DefaultResourceRepository<TodoItem>(
                _targetedFieldsMock.Object,
                _contextResolverMock.Object,
                resourceGraph, null, null);
        }
    }
}
