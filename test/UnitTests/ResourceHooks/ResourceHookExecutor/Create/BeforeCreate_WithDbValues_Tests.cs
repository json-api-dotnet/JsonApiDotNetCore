using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public sealed class BeforeCreate_WithDbValues_Tests : HooksTestsSetup
    {
        private readonly ResourceHook[] _targetHooks = { ResourceHook.BeforeCreate, ResourceHook.BeforeImplicitUpdateRelationship, ResourceHook.BeforeUpdateRelationship };
        private readonly ResourceHook[] _targetHooksNoImplicit = { ResourceHook.BeforeCreate, ResourceHook.BeforeUpdateRelationship };

        private const string _description = "DESCRIPTION";
        private const string _lastName = "NAME";
        private readonly string _personId;
        private readonly List<TodoItem> _todoList;
        private readonly DbContextOptions<AppDbContext> _options;

        public BeforeCreate_WithDbValues_Tests()
        {
            _todoList = CreateTodoWithToOnePerson();

            _todoList[0].Id = 0;
            _todoList[0].Description = _description;
            var person = _todoList[0].OneToOnePerson;
            person.LastName = _lastName;
            _personId = person.Id.ToString();
            var implicitTodo = _todoFaker.Generate();
            implicitTodo.Id += 1000;
            implicitTodo.OneToOnePerson = person;
            implicitTodo.Description = _description + _description;

            _options = InitInMemoryDb(context =>
            {
                context.Set<Person>().Add(person);
                context.Set<TodoItem>().Add(implicitTodo);
                context.SaveChanges();
            });

            _todoList[0].OneToOnePerson = person;
        }

        [Fact]
        public void BeforeCreate()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, EnableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, EnableDbValues);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: _options);

            // Act
            hookExecutor.BeforeCreate(_todoList, ResourcePipeline.Post);

            // Assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(It.Is<IResourceHashSet<TodoItem>>((resources) => TodoCheck(resources, _description)), ResourcePipeline.Post), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(
                It.Is<HashSet<string>>(ids => PersonIdCheck(ids, _personId)),
                It.IsAny<IRelationshipsDictionary<Person>>(),
                ResourcePipeline.Post),
                Times.Once());
            todoResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(
                It.Is<IRelationshipsDictionary<TodoItem>>(rh => TodoCheckRelationships(rh, _description + _description)),
                ResourcePipeline.Post),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_Without_Parent_Hook_Implemented()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, EnableDbValues);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: _options);

            // Act
            hookExecutor.BeforeCreate(_todoList, ResourcePipeline.Post);

            // Assert
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(
                It.Is<HashSet<string>>(ids => PersonIdCheck(ids, _personId)),
                It.IsAny<IRelationshipsDictionary<Person>>(),
                ResourcePipeline.Post),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_Without_Child_Hook_Implemented()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, EnableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: _options);

            // Act
            hookExecutor.BeforeCreate(_todoList, ResourcePipeline.Post);

            // Assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(It.Is<IResourceHashSet<TodoItem>>((resources) => TodoCheck(resources, _description)), ResourcePipeline.Post), Times.Once());
            todoResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(
                It.Is<IRelationshipsDictionary<TodoItem>>(rh => TodoCheckRelationships(rh, _description + _description)),
                ResourcePipeline.Post),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_NoImplicit()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooksNoImplicit, ResourceHook.BeforeUpdate);
            var personDiscovery = SetDiscoverableHooks<Person>(_targetHooksNoImplicit, ResourceHook.BeforeUpdateRelationship);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: _options);

            // Act
            hookExecutor.BeforeCreate(_todoList, ResourcePipeline.Post);

            // Assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(It.Is<IResourceHashSet<TodoItem>>((resources) => TodoCheck(resources, _description)), ResourcePipeline.Post), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(
                It.Is<HashSet<string>>(ids => PersonIdCheck(ids, _personId)),
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
            var personDiscovery = SetDiscoverableHooks<Person>(_targetHooksNoImplicit, ResourceHook.BeforeUpdateRelationship);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: _options);

            // Act
            hookExecutor.BeforeCreate(_todoList, ResourcePipeline.Post);

            // Assert
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(
                It.Is<HashSet<string>>(ids => PersonIdCheck(ids, _personId)),
                It.IsAny<IRelationshipsDictionary<Person>>(),
                ResourcePipeline.Post),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_NoImplicit_Without_Child_Hook_Implemented()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooksNoImplicit, ResourceHook.BeforeUpdate);
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: _options);

            // Act
            hookExecutor.BeforeCreate(_todoList, ResourcePipeline.Post);

            // Assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(It.Is<IResourceHashSet<TodoItem>>((resources) => TodoCheck(resources, _description)), ResourcePipeline.Post), Times.Once());
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
