using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace UnitTests.Data
{
    public sealed class EntityFrameworkCoreRepositoryTests
    {
        [Fact]
        public async Task UpdateAsync_AttributesUpdated_ShouldHaveSpecificallyThoseAttributesUpdated()
        {
            // Arrange
            var itemId = 213;
            var seed = Guid.NewGuid();

            var databaseResource = new TodoItem
            {
                Id = itemId,
                Description = "Before"
            };

            var todoItemUpdates = new TodoItem
            {
                Id = itemId,
                Description = "After"
            };

            await using (var arrangeDbContext = GetDbContext(seed))
            {
                var (repository, targetedFields, _) = Setup(arrangeDbContext);
                arrangeDbContext.Add(databaseResource);
                await arrangeDbContext.SaveChangesAsync();

                var descAttr = new AttrAttribute
                {
                    PublicName = "description",
                    Property = typeof(TodoItem).GetProperty(nameof(TodoItem.Description))
                };
                targetedFields.Setup(m => m.Attributes).Returns(new HashSet<AttrAttribute> { descAttr });
                targetedFields.Setup(m => m.Relationships).Returns(new HashSet<RelationshipAttribute>());

                // Act
                await repository.UpdateAsync(todoItemUpdates, databaseResource, CancellationToken.None);
            }

            // Assert - in different context
            await using (var assertDbContext = GetDbContext(seed))
            {
                var (repository, _, resourceGraph) = Setup(assertDbContext);

                var resourceContext = resourceGraph.GetResourceContext<TodoItem>();
                var idAttribute = resourceContext.Attributes.Single(a => a.Property.Name == nameof(Identifiable.Id));

                var resources = await repository.GetAsync(new QueryLayer(resourceContext)
                {
                    Filter = new ComparisonExpression(ComparisonOperator.Equals,
                        new ResourceFieldChainExpression(idAttribute),
                        new LiteralConstantExpression(itemId.ToString()))
                }, CancellationToken.None);

                var fetchedTodo = resources.First();
                Assert.NotNull(fetchedTodo);
                Assert.Equal(databaseResource.Ordinal, fetchedTodo.Ordinal);
                Assert.Equal(todoItemUpdates.Description, fetchedTodo.Description);
            }
        }

        private (EntityFrameworkCoreRepository<TodoItem> Repository, Mock<ITargetedFields> TargetedFields, IResourceGraph resourceGraph) Setup(AppDbContext context)
        {
            var serviceProvider = ((IInfrastructure<IServiceProvider>) context).Instance;
            var resourceFactory = new ResourceFactory(serviceProvider);
            var contextResolverMock = new Mock<IDbContextResolver>();
            contextResolverMock.Setup(m => m.GetContext()).Returns(context);
            var resourceGraph = new ResourceGraphBuilder(new JsonApiOptions(), NullLoggerFactory.Instance).Add<TodoItem>().Build();
            var targetedFields = new Mock<ITargetedFields>();
            
            var repository = new EntityFrameworkCoreRepository<TodoItem>(targetedFields.Object,
                contextResolverMock.Object, resourceGraph, resourceFactory, new List<IQueryConstraintProvider>(), NullLoggerFactory.Instance);
            
            return (repository, targetedFields, resourceGraph);
        }

        private AppDbContext GetDbContext(Guid? seed = null)
        {
            Guid actualSeed = seed ?? Guid.NewGuid();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"IntegrationDatabaseRepository{actualSeed}")
                .Options;
            var context = new AppDbContext(options, new FrozenSystemClock());

            context.RemoveRange(context.TodoItems);
            return context;
        }
    }
}
