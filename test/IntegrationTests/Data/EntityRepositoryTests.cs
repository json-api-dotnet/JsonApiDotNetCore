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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;


namespace JADNC.IntegrationTests.Data
{
    public class EntityRepositoryTests
    {


        public EntityRepositoryTests()
        {
        }

        [Fact]
        public async Task UpdateAsync_AttributesUpdated_ShouldHaveSpecificallyThoseAttributesUpdated()
        {
            // Arrange
            var itemId = 213;
            using (var arrangeContext = GetContext())
            {
                var (repository, targetedFields) = Setup(arrangeContext);
                var todoItemUpdates = new TodoItem
                {
                    Id = itemId,
                    Description = Guid.NewGuid().ToString()
                };

                var descAttr = new AttrAttribute("description", "Description")
                {
                    PropertyInfo = typeof(TodoItem).GetProperty(nameof(TodoItem.Description))
                };
                targetedFields.Setup(m => m.Attributes).Returns(new List<AttrAttribute> { descAttr });
                targetedFields.Setup(m => m.Relationships).Returns(new List<RelationshipAttribute>());

                // Act
                var updatedItem = await repository.UpdateAsync(todoItemUpdates);
            }

            // Assert - in different context
            using var assertContext = GetContext();
            {
                var (repository, targetedFields) = Setup(assertContext);

                var fetchedTodo = repository.Get(itemId).First();
                Assert.NotNull(fetchedTodo);
                Assert.Equal(fetchedTodo.Ordinal, fetchedTodo.Ordinal);
                Assert.Equal(fetchedTodo.Description, fetchedTodo.Description);

            }

        }
        [Theory]
        [InlineData(6, -1, new[] { 4, 5, 6, 7, 8, 9 })]
        [InlineData(6, -2, new[] { 1, 2, 3 })]
        [InlineData(20, -1, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
        public async Task Paging_PageNumberIsNegative_GiveBackReverseAmountOfIds(int pageSize, int pageNumber, int[] expectedIds)
        {
            // Arrange
            using var context = GetContext();
            var repository = Setup(context).Repository;
            context.AddRange(TodoItems(1, 2, 3, 4, 5, 6, 7, 8, 9));
            context.SaveChanges();

            // Act
            var result = await repository.PageAsync(context.Set<TodoItem>(), pageSize, pageNumber);

            // Assert
            Assert.Equal(TodoItems(expectedIds), result, new IdComparer<TodoItem>());
        }


        private (DefaultResourceRepository<TodoItem> Repository, Mock<ITargetedFields> TargetedFields) Setup(AppDbContext context)
        {
            var contextResolverMock = new Mock<IDbContextResolver>();
            contextResolverMock.Setup(m => m.GetContext()).Returns(context);
            var resourceGraph = new ResourceGraphBuilder().AddResource<TodoItem>().Build();
            var targetedFields = new Mock<ITargetedFields>();
            var repository = new DefaultResourceRepository<TodoItem>(targetedFields.Object, contextResolverMock.Object, resourceGraph, null);
            return (repository, targetedFields);
        }

        private AppDbContext GetContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "IntegrationDatabaseRepository")
                .Options;
            var context = new AppDbContext(options);

            context.TodoItems.RemoveRange(context.TodoItems.ToList());
            return context;
        }
        private static TodoItem[] TodoItems(params int[] ids)
        {
            return ids.Select(id => new TodoItem { Id = id }).ToArray();
        }

        private class IdComparer<T> : IEqualityComparer<T>
            where T : IIdentifiable
        {
            public bool Equals(T x, T y) => x?.StringId == y?.StringId;

            public int GetHashCode(T obj) => obj?.StringId?.GetHashCode() ?? 0;
        }
    }
}
