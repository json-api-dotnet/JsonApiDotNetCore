using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public sealed class BeforeCreate_WithDbValues_Tests : HooksTestsSetup
    {
        private readonly ResourceHook[] targetHooks = { ResourceHook.BeforeCreate, ResourceHook.BeforeImplicitUpdateRelationship, ResourceHook.BeforeUpdateRelationship };
        private readonly ResourceHook[] targetHooksNoImplicit = { ResourceHook.BeforeCreate, ResourceHook.BeforeUpdateRelationship };

        private readonly string description = "DESCRIPTION";
        private readonly string lastName = "NAME";
        private readonly string personId;
        private readonly List<TodoItem> todoList;
        private readonly DbContextOptions<AppDbContext> options;

        public BeforeCreate_WithDbValues_Tests()
        {
            todoList = CreateTodoWithToOnePerson();

            todoList[0].Id = 0;
            todoList[0].Description = description;
            var _personId = todoList[0].OneToOnePerson.Id;
            personId = _personId.ToString();
            var implicitTodo = _todoFaker.Generate();
            implicitTodo.Id += 1000;
            implicitTodo.OneToOnePersonId = _personId;
            implicitTodo.Description = description + description;

            options = InitInMemoryDb(context =>
            {
                context.Set<Person>().Add(new Person { Id = _personId, LastName = lastName });
                context.Set<TodoItem>().Add(implicitTodo);
                context.SaveChanges();
            });
        }

        [Fact]
        public void BeforeCreate()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, EnableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, EnableDbValues);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            // Act
            hookExecutor.BeforeCreate(todoList, ResourcePipeline.Post);

            // Assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(It.Is<IResourceHashSet<TodoItem>>((resources) => TodoCheck(resources, description)), ResourcePipeline.Post), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(
                It.Is<HashSet<string>>(ids => PersonIdCheck(ids, personId)),
                It.IsAny<IRelationshipsDictionary<Person>>(),
                ResourcePipeline.Post),
                Times.Once());
            todoResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(
                It.Is<IRelationshipsDictionary<TodoItem>>(rh => TodoCheckRelationships(rh, description + description)),
                ResourcePipeline.Post),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_Without_Parent_Hook_Implemented()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, EnableDbValues);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            // Act
            hookExecutor.BeforeCreate(todoList, ResourcePipeline.Post);

            // Assert
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(
                It.Is<HashSet<string>>(ids => PersonIdCheck(ids, personId)),
                It.IsAny<IRelationshipsDictionary<Person>>(),
                ResourcePipeline.Post),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_Without_Child_Hook_Implemented()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, EnableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            // Act
            hookExecutor.BeforeCreate(todoList, ResourcePipeline.Post);

            // Assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(It.Is<IResourceHashSet<TodoItem>>((resources) => TodoCheck(resources, description)), ResourcePipeline.Post), Times.Once());
            todoResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(
                It.Is<IRelationshipsDictionary<TodoItem>>(rh => TodoCheckRelationships(rh, description + description)),
                ResourcePipeline.Post),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_NoImplicit()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooksNoImplicit, ResourceHook.BeforeUpdate);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooksNoImplicit, ResourceHook.BeforeUpdateRelationship);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            // Act
            hookExecutor.BeforeCreate(todoList, ResourcePipeline.Post);

            // Assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(It.Is<IResourceHashSet<TodoItem>>((resources) => TodoCheck(resources, description)), ResourcePipeline.Post), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(
                It.Is<HashSet<string>>(ids => PersonIdCheck(ids, personId)),
                It.IsAny<IRelationshipsDictionary<Person>>(),
                ResourcePipeline.Post),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_NoImplicit_Without_Parent_Hook_Implemented()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooksNoImplicit, ResourceHook.BeforeUpdateRelationship);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            // Act
            hookExecutor.BeforeCreate(todoList, ResourcePipeline.Post);

            // Assert
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(
                It.Is<HashSet<string>>(ids => PersonIdCheck(ids, personId)),
                It.IsAny<IRelationshipsDictionary<Person>>(),
                ResourcePipeline.Post),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_NoImplicit_Without_Child_Hook_Implemented()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooksNoImplicit, ResourceHook.BeforeUpdate);
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            // Act
            hookExecutor.BeforeCreate(todoList, ResourcePipeline.Post);

            // Assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(It.Is<IResourceHashSet<TodoItem>>((resources) => TodoCheck(resources, description)), ResourcePipeline.Post), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        private bool TodoCheck(IEnumerable<TodoItem> resources, string checksum)
        {
            return resources.Single().Description == checksum;
        }

        private bool TodoCheckRelationships(IRelationshipsDictionary<TodoItem> rh, string checksum)
        {
            return rh.GetByRelationship<Person>().Single().Value.First().Description == checksum;
        }

        private bool PersonIdCheck(IEnumerable<string> ids, string checksum)
        {
            return ids.Single() == checksum;
        }
    }
}
