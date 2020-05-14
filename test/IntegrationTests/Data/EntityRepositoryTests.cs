using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntegrationTests;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace JADNC.IntegrationTests.Data
{
    public sealed class EntityRepositoryTests
    {
        [Fact]
        public async Task UpdateAsync_AttributesUpdated_ShouldHaveSpecificallyThoseAttributesUpdated()
        {
            // Arrange
            var itemId = 213;
            var seed = Guid.NewGuid();

            var databaseEntity = new TodoItem
            {
                Id = itemId,
                Description = "Before"
            };

            var todoItemUpdates = new TodoItem
            {
                Id = itemId,
                Description = "After"
            };

            await using (var arrangeContext = GetContext(seed))
            {
                var (repository, targetedFields) = Setup(arrangeContext);
                arrangeContext.Add(databaseEntity);
                arrangeContext.SaveChanges();

                var descAttr = new AttrAttribute("description")
                {
                    PropertyInfo = typeof(TodoItem).GetProperty(nameof(TodoItem.Description))
                };
                targetedFields.Setup(m => m.Attributes).Returns(new List<AttrAttribute> { descAttr });
                targetedFields.Setup(m => m.Relationships).Returns(new List<RelationshipAttribute>());

                // Act
                await repository.UpdateAsync(todoItemUpdates, databaseEntity);
            }

            // Assert - in different context
            await using var assertContext = GetContext(seed);
            {
                var (repository, _) = Setup(assertContext);

                var fetchedTodo = repository.Get(itemId).First();
                Assert.NotNull(fetchedTodo);
                Assert.Equal(databaseEntity.Ordinal, fetchedTodo.Ordinal);
                Assert.Equal(todoItemUpdates.Description, fetchedTodo.Description);

            }
        }

        [Theory]
        [InlineData(3, 2, new[] { 4, 5, 6 })]
        [InlineData(8, 2, new[] { 9 })]
        [InlineData(20, 1, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
        public async Task Paging_PageNumberIsPositive_ReturnCorrectIdsAtTheFront(int pageSize, int pageNumber, int[] expectedResult)
        {
            // Arrange
            await using var context = GetContext();
            var (repository, _) = Setup(context);
            context.AddRange(TodoItems(1, 2, 3, 4, 5, 6, 7, 8, 9).Cast<object>());
            await context.SaveChangesAsync();

            // Act
            var result = await repository.PageAsync(context.Set<TodoItem>(), pageSize, pageNumber);

            // Assert
            Assert.Equal(TodoItems(expectedResult), result, new IdComparer<TodoItem>());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public async Task Paging_PageSizeNonPositive_DoNothing(int pageSize)
        {
            // Arrange
            await using var context = GetContext();
            var (repository, _) = Setup(context);
            var items = TodoItems(2, 3, 1);
            context.AddRange(items.Cast<object>());
            await context.SaveChangesAsync();

            // Act
            var result = await repository.PageAsync(context.Set<TodoItem>(), pageSize, 3);

            // Assert
            Assert.Equal(items.ToList(), result.ToList(), new IdComparer<TodoItem>());
        }

        [Fact]
        public async Task Paging_PageNumberDoesNotExist_ReturnEmptyAQueryable()
        {
            // Arrange
            var items = TodoItems(2, 3, 1);
            await using var context = GetContext();
            var (repository, _) = Setup(context);
            context.AddRange(items.Cast<object>());

            // Act
            var result = await repository.PageAsync(context.Set<TodoItem>(), 2, 3);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task Paging_PageNumberIsZero_PretendsItsOne()
        {
            // Arrange
            await using var context = GetContext();
            var (repository, _) = Setup(context);
            context.AddRange(TodoItems(2, 3, 4, 5, 6, 7, 8, 9).Cast<object>());
            await context.SaveChangesAsync();

            // Act
            var result = await repository.PageAsync(entities: context.Set<TodoItem>(), pageSize: 1, pageNumber: 0);

            // Assert
            Assert.Equal(TodoItems(2), result, new IdComparer<TodoItem>());
        }

        [Theory]
        [InlineData(6, -1, new[] { 9, 8, 7, 6, 5, 4 })]
        [InlineData(6, -2, new[] { 3, 2, 1 })]
        [InlineData(20, -1, new[] { 9, 8, 7, 6, 5, 4, 3, 2, 1 })]
        public async Task Paging_PageNumberIsNegative_GiveBackReverseAmountOfIds(int pageSize, int pageNumber, int[] expectedIds)
        {
            // Arrange
            await using var context = GetContext();
            var repository = Setup(context).Repository;
            context.AddRange(TodoItems(1, 2, 3, 4, 5, 6, 7, 8, 9).Cast<object>());
            context.SaveChanges();

            // Act
            var result = await repository.PageAsync(context.Set<TodoItem>(), pageSize, pageNumber);

            // Assert
            Assert.Equal(TodoItems(expectedIds), result, new IdComparer<TodoItem>());
        }


        private (DefaultResourceRepository<TodoItem> Repository, Mock<ITargetedFields> TargetedFields) Setup(AppDbContext context)
        {
            var serviceProvider = ((IInfrastructure<IServiceProvider>) context).Instance;
            var resourceFactory = new DefaultResourceFactory(serviceProvider);
            var contextResolverMock = new Mock<IDbContextResolver>();
            contextResolverMock.Setup(m => m.GetContext()).Returns(context);
            var resourceGraph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance).AddResource<TodoItem>().Build();
            var targetedFields = new Mock<ITargetedFields>();
            var repository = new DefaultResourceRepository<TodoItem>(targetedFields.Object, contextResolverMock.Object, resourceGraph, null, resourceFactory, NullLoggerFactory.Instance);
            return (repository, targetedFields);
        }

        private AppDbContext GetContext(Guid? seed = null)
        {
            Guid actualSeed = seed ?? Guid.NewGuid();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"IntegrationDatabaseRepository{actualSeed}")
                .Options;
            var context = new AppDbContext(options, new FrozenSystemClock());

            context.TodoItems.RemoveRange(context.TodoItems);
            return context;
        }

        private static TodoItem[] TodoItems(params int[] ids)
        {
            return ids.Select(id => new TodoItem { Id = id }).ToArray();
        }

        private sealed class IdComparer<T> : IEqualityComparer<T>
            where T : IIdentifiable
        {
            public bool Equals(T x, T y) => x?.StringId == y?.StringId;

            public int GetHashCode(T obj) => obj.StringId?.GetHashCode() ?? 0;
        }
    }
}
