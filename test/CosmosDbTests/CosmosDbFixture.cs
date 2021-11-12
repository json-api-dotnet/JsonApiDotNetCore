using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CosmosDbExample;
using CosmosDbExample.Data;
using CosmosDbExample.Models;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

#pragma warning disable AV1115 // Member or local function contains the word 'and', which suggests doing multiple things

namespace CosmosDbTests
{
    /// <summary>
    /// Test fixture that deletes and recreates the TodoItemDB database and PeopleAndTodoItems container before all of the test methods are run. It does not
    /// delete the database nor the container at the end.
    /// </summary>
    [PublicAPI]
    public class CosmosDbFixture : WebApplicationFactory<Startup>, IAsyncLifetime
    {
        public Guid BonnieId { get; } = Guid.NewGuid();

        public Guid ClydeId { get; } = Guid.NewGuid();

        public Guid TodoItemOwnedByBonnieId { get; } = Guid.NewGuid();

        public Guid TodoItemOwnedByClydeId { get; } = Guid.NewGuid();

        public Guid TodoItemWithOwnerAndAssigneeId { get; } = Guid.NewGuid();

        public async Task InitializeAsync()
        {
            await RunOnDatabaseAsync(async context =>
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                Person bonnie = new()
                {
                    Id = BonnieId,
                    FirstName = "Bonnie",
                    LastName = "Parker"
                };

                Person clyde = new()
                {
                    Id = ClydeId,
                    FirstName = "Clyde",
                    LastName = "Barrow"
                };

                TodoItem todoItemOwnedByBonnie = new()
                {
                    Id = TodoItemOwnedByBonnieId,
                    Description = "Rob bank",
                    Priority = TodoItemPriority.High,
                    OwnerId = bonnie.Id,
                    Tags = new HashSet<Tag>
                    {
                        new()
                        {
                            Name = "Job"
                        }
                    }
                };

                TodoItem todoItemOwnedByClyde = new()
                {
                    Id = TodoItemOwnedByClydeId,
                    Description = "Wash car",
                    Priority = TodoItemPriority.Low,
                    OwnerId = clyde.Id
                };

                TodoItem todoItemWithOwnerAndAssignee = new()
                {
                    Id = TodoItemWithOwnerAndAssigneeId,
                    Description = "Go shopping",
                    Priority = TodoItemPriority.Medium,
                    OwnerId = clyde.Id,
                    AssigneeId = bonnie.Id,
                    Tags = new HashSet<Tag>
                    {
                        new()
                        {
                            Name = "Errands"
                        },
                        new()
                        {
                            Name = "Groceries"
                        }
                    }
                };

                // @formatter:off

                context.People.AddRange(bonnie, clyde);
                context.TodoItems.AddRange(todoItemOwnedByBonnie, todoItemOwnedByClyde, todoItemWithOwnerAndAssignee);

                // @formatter:on

                await context.SaveChangesAsync();
            });
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task<TEntity> CreateEntityAsync<TEntity>(TEntity entity)
            where TEntity : IIdentifiable
        {
            await RunOnDatabaseAsync(async context =>
            {
                // ReSharper disable once MethodHasAsyncOverload
                context.Add(entity);
                await context.SaveChangesAsync();
            });

            return entity;
        }

        public async Task RunOnDatabaseAsync(Func<AppDbContext, Task> asyncAction)
        {
            using IServiceScope scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await asyncAction(dbContext);
        }
    }
}
