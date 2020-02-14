using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace UnitTests.Data
{
    public class DefaultEntityRepositoryTest
    {

        [Fact]
        public async Task PageAsync_IQueryableIsAListAndPageNumberPositive_CanStillCount()
        {
            // If IQueryable is actually a list (this can happen after a filter or hook)
            // It needs to not do CountAsync, because well.. its not asynchronous.

            // Arrange
            var repository = Setup();
            var todoItems = new List<TodoItem>
            {
                new TodoItem{ Id = 1 },
                new TodoItem{ Id = 2 }
            };

            // Act
            var result = await repository.PageAsync(todoItems.AsQueryable(), pageSize: 1, pageNumber: 2);

            // Assert
            Assert.True(result.ElementAt(0).Id == todoItems[1].Id);
        }

        [Fact]
        public async Task PageAsync_IQueryableIsAListAndPageNumberNegative_CanStillCount()
        {
            // If IQueryable is actually a list (this can happen after a filter or hook)
            // It needs to not do CountAsync, because well.. its not asynchronous.

            // Arrange
            var repository = Setup();
            var todoItems = new List<TodoItem>
            {
                new TodoItem{ Id = 1 },
                new TodoItem{ Id = 2 },
                new TodoItem{ Id = 3 },
                new TodoItem{ Id = 4 }
            };

            // Act
            var result = await repository.PageAsync(todoItems.AsQueryable(), pageSize: 1, pageNumber: -2);

            // Assert
            Assert.True(result.First().Id == 3);
        }

        private DefaultResourceRepository<TodoItem> Setup()
        {
            var contextResolverMock = new Mock<IDbContextResolver>();
            contextResolverMock.Setup(m => m.GetContext()).Returns(new Mock<DbContext>().Object);
            var resourceGraph = new Mock<IResourceGraph>();
            var targetedFields = new Mock<ITargetedFields>();
            var repository = new DefaultResourceRepository<TodoItem>(targetedFields.Object, contextResolverMock.Object, resourceGraph.Object, null, NullLoggerFactory.Instance);
            return repository;
        }

    }
}
