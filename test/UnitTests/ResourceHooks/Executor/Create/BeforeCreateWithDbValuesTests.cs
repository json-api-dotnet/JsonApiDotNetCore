using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Hooks.Internal;
using JsonApiDotNetCore.Hooks.Internal.Discovery;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace UnitTests.ResourceHooks.Executor.Create
{
    public sealed class BeforeCreateWithDbValuesTests : HooksTestsSetup
    {
        private const string Description = "DESCRIPTION";
        private const string LastName = "NAME";

        private readonly ResourceHook[] _targetHooks =
        {
            ResourceHook.BeforeCreate,
            ResourceHook.BeforeImplicitUpdateRelationship,
            ResourceHook.BeforeUpdateRelationship
        };

        private readonly ResourceHook[] _targetHooksNoImplicit =
        {
            ResourceHook.BeforeCreate,
            ResourceHook.BeforeUpdateRelationship
        };

        private readonly string _personId;
        private readonly IList<TodoItem> _todoList;
        private readonly DbContextOptions<AppDbContext> _options;

        public BeforeCreateWithDbValuesTests()
        {
            _todoList = CreateTodoWithToOnePerson();

            _todoList[0].Id = 0;
            _todoList[0].Description = Description;
            Person person = _todoList[0].OneToOnePerson;
            person.LastName = LastName;
            _personId = person.Id.ToString();
            TodoItem implicitTodo = TodoFaker.Generate();
            implicitTodo.Id += 1000;
            implicitTodo.OneToOnePerson = person;
            implicitTodo.Description = Description + Description;

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
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, EnableDbValues);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, EnableDbValues);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock,
                Mock<IResourceHookContainer<Person>> ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, _options);

            // Act
            hookExecutor.BeforeCreate(_todoList, ResourcePipeline.Post);

            // Assert
            todoResourceMock.Verify(
                rd => rd.BeforeCreate(It.Is<IResourceHashSet<TodoItem>>(resources => TodoCheck(resources, Description)), ResourcePipeline.Post), Times.Once());

            ownerResourceMock.Verify(
                rd => rd.BeforeUpdateRelationship(It.Is<HashSet<string>>(ids => PersonIdCheck(ids, _personId)), It.IsAny<IRelationshipsDictionary<Person>>(),
                    ResourcePipeline.Post), Times.Once());

            todoResourceMock.Verify(
                rd => rd.BeforeImplicitUpdateRelationship(
                    It.Is<IRelationshipsDictionary<TodoItem>>(rh => TodoCheckRelationships(rh, Description + Description)), ResourcePipeline.Post),
                Times.Once());

            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_Without_Parent_Hook_Implemented()
        {
            // Arrange
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, EnableDbValues);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock,
                Mock<IResourceHookContainer<Person>> ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, _options);

            // Act
            hookExecutor.BeforeCreate(_todoList, ResourcePipeline.Post);

            // Assert
            ownerResourceMock.Verify(
                rd => rd.BeforeUpdateRelationship(It.Is<HashSet<string>>(ids => PersonIdCheck(ids, _personId)), It.IsAny<IRelationshipsDictionary<Person>>(),
                    ResourcePipeline.Post), Times.Once());

            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_Without_Child_Hook_Implemented()
        {
            // Arrange
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, EnableDbValues);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock,
                Mock<IResourceHookContainer<Person>> ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, _options);

            // Act
            hookExecutor.BeforeCreate(_todoList, ResourcePipeline.Post);

            // Assert
            todoResourceMock.Verify(
                rd => rd.BeforeCreate(It.Is<IResourceHashSet<TodoItem>>(resources => TodoCheck(resources, Description)), ResourcePipeline.Post), Times.Once());

            todoResourceMock.Verify(
                rd => rd.BeforeImplicitUpdateRelationship(
                    It.Is<IRelationshipsDictionary<TodoItem>>(rh => TodoCheckRelationships(rh, Description + Description)), ResourcePipeline.Post),
                Times.Once());

            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_NoImplicit()
        {
            // Arrange
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooksNoImplicit, ResourceHook.BeforeUpdate);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(_targetHooksNoImplicit, ResourceHook.BeforeUpdateRelationship);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock,
                Mock<IResourceHookContainer<Person>> ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, _options);

            // Act
            hookExecutor.BeforeCreate(_todoList, ResourcePipeline.Post);

            // Assert
            todoResourceMock.Verify(
                rd => rd.BeforeCreate(It.Is<IResourceHashSet<TodoItem>>(resources => TodoCheck(resources, Description)), ResourcePipeline.Post), Times.Once());

            ownerResourceMock.Verify(
                rd => rd.BeforeUpdateRelationship(It.Is<HashSet<string>>(ids => PersonIdCheck(ids, _personId)), It.IsAny<IRelationshipsDictionary<Person>>(),
                    ResourcePipeline.Post), Times.Once());

            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_NoImplicit_Without_Parent_Hook_Implemented()
        {
            // Arrange
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(_targetHooksNoImplicit, ResourceHook.BeforeUpdateRelationship);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock,
                Mock<IResourceHookContainer<Person>> ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, _options);

            // Act
            hookExecutor.BeforeCreate(_todoList, ResourcePipeline.Post);

            // Assert
            ownerResourceMock.Verify(
                rd => rd.BeforeUpdateRelationship(It.Is<HashSet<string>>(ids => PersonIdCheck(ids, _personId)), It.IsAny<IRelationshipsDictionary<Person>>(),
                    ResourcePipeline.Post), Times.Once());

            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_NoImplicit_Without_Child_Hook_Implemented()
        {
            // Arrange
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooksNoImplicit, ResourceHook.BeforeUpdate);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock,
                Mock<IResourceHookContainer<Person>> ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, _options);

            // Act
            hookExecutor.BeforeCreate(_todoList, ResourcePipeline.Post);

            // Assert
            todoResourceMock.Verify(
                rd => rd.BeforeCreate(It.Is<IResourceHashSet<TodoItem>>(resources => TodoCheck(resources, Description)), ResourcePipeline.Post), Times.Once());

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
