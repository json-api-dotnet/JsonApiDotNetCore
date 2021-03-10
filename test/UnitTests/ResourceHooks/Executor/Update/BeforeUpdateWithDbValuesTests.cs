using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Hooks.Internal;
using JsonApiDotNetCore.Hooks.Internal.Discovery;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace UnitTests.ResourceHooks.Executor.Update
{
    public sealed class BeforeUpdateWithDbValuesTests : HooksTestsSetup
    {
        private const string Description = "DESCRIPTION";
        private const string LastName = "NAME";

        private readonly ResourceHook[] _targetHooks =
        {
            ResourceHook.BeforeUpdate,
            ResourceHook.BeforeImplicitUpdateRelationship,
            ResourceHook.BeforeUpdateRelationship
        };

        private readonly ResourceHook[] _targetHooksNoImplicit =
        {
            ResourceHook.BeforeUpdate,
            ResourceHook.BeforeUpdateRelationship
        };

        private readonly string _personId;
        private readonly IList<TodoItem> _todoList;
        private readonly DbContextOptions<AppDbContext> _options;

        public BeforeUpdateWithDbValuesTests()
        {
            _todoList = CreateTodoWithToOnePerson();

            int todoId = _todoList[0].Id;
            int personId = _todoList[0].OneToOnePerson.Id;
            _personId = personId.ToString();
            int implicitPersonId = personId + 10000;

            TodoItem implicitTodo = TodoFaker.Generate();
            implicitTodo.Id += 1000;

            implicitTodo.OneToOnePerson = new Person
            {
                Id = personId,
                LastName = LastName
            };

            implicitTodo.Description = Description + Description;

            _options = InitInMemoryDb(context =>
            {
                context.Set<TodoItem>().Add(new TodoItem
                {
                    Id = todoId,
                    OneToOnePerson = new Person
                    {
                        Id = implicitPersonId,
                        LastName = LastName + LastName
                    },
                    Description = Description
                });

                context.Set<TodoItem>().Add(implicitTodo);
                context.SaveChanges();
            });
        }

        [Fact]
        public void BeforeUpdate()
        {
            // Arrange
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, EnableDbValues);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, EnableDbValues);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock,
                Mock<IResourceHookContainer<Person>> ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, _options);

            // Act
            hookExecutor.BeforeUpdate(_todoList, ResourcePipeline.Patch);

            // Assert
            todoResourceMock.Verify(
                rd => rd.BeforeUpdate(It.Is<IDiffableResourceHashSet<TodoItem>>(diff => TodoCheckDiff(diff, Description)), ResourcePipeline.Patch),
                Times.Once());

            ownerResourceMock.Verify(
                rd => rd.BeforeUpdateRelationship(It.Is<HashSet<string>>(ids => PersonIdCheck(ids, _personId)),
                    It.Is<IRelationshipsDictionary<Person>>(rh => PersonCheck(LastName, rh)), ResourcePipeline.Patch), Times.Once());

            ownerResourceMock.Verify(
                rd => rd.BeforeImplicitUpdateRelationship(It.Is<IRelationshipsDictionary<Person>>(rh => PersonCheck(LastName + LastName, rh)),
                    ResourcePipeline.Patch), Times.Once());

            todoResourceMock.Verify(
                rd => rd.BeforeImplicitUpdateRelationship(It.Is<IRelationshipsDictionary<TodoItem>>(rh => TodoCheck(rh, Description + Description)),
                    ResourcePipeline.Patch), Times.Once());

            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeUpdate_Deleting_Relationship()
        {
            // Arrange
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, EnableDbValues);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, EnableDbValues);

            (_, Mock<ITargetedFields> ufMock, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock,
                Mock<IResourceHookContainer<Person>> ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, _options);

            ufMock.Setup(targetedFields => targetedFields.Relationships)
                .Returns(ResourceGraph.GetRelationships((TodoItem todoItem) => todoItem.OneToOnePerson).ToHashSet);

            // Act
            var todoList = new List<TodoItem>
            {
                new TodoItem
                {
                    Id = _todoList[0].Id
                }
            };

            hookExecutor.BeforeUpdate(todoList, ResourcePipeline.Patch);

            // Assert
            todoResourceMock.Verify(
                rd => rd.BeforeUpdate(It.Is<IDiffableResourceHashSet<TodoItem>>(diff => TodoCheckDiff(diff, Description)), ResourcePipeline.Patch),
                Times.Once());

            ownerResourceMock.Verify(
                rd => rd.BeforeImplicitUpdateRelationship(It.Is<IRelationshipsDictionary<Person>>(rh => PersonCheck(LastName + LastName, rh)),
                    ResourcePipeline.Patch), Times.Once());

            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeUpdate_Without_Parent_Hook_Implemented()
        {
            // Arrange
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, EnableDbValues);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock,
                Mock<IResourceHookContainer<Person>> ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, _options);

            // Act
            hookExecutor.BeforeUpdate(_todoList, ResourcePipeline.Patch);

            // Assert
            ownerResourceMock.Verify(
                rd => rd.BeforeUpdateRelationship(It.Is<HashSet<string>>(ids => PersonIdCheck(ids, _personId)),
                    It.Is<IRelationshipsDictionary<Person>>(rh => PersonCheck(LastName, rh)), ResourcePipeline.Patch), Times.Once());

            ownerResourceMock.Verify(
                rd => rd.BeforeImplicitUpdateRelationship(It.Is<IRelationshipsDictionary<Person>>(rh => PersonCheck(LastName + LastName, rh)),
                    ResourcePipeline.Patch), Times.Once());

            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeUpdate_Without_Child_Hook_Implemented()
        {
            // Arrange
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, EnableDbValues);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock,
                Mock<IResourceHookContainer<Person>> ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, _options);

            // Act
            hookExecutor.BeforeUpdate(_todoList, ResourcePipeline.Patch);

            // Assert
            todoResourceMock.Verify(
                rd => rd.BeforeUpdate(It.Is<IDiffableResourceHashSet<TodoItem>>(diff => TodoCheckDiff(diff, Description)), ResourcePipeline.Patch),
                Times.Once());

            todoResourceMock.Verify(
                rd => rd.BeforeImplicitUpdateRelationship(It.Is<IRelationshipsDictionary<TodoItem>>(rh => TodoCheck(rh, Description + Description)),
                    ResourcePipeline.Patch), Times.Once());

            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeUpdate_NoImplicit()
        {
            // Arrange
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooksNoImplicit, ResourceHook.BeforeUpdate);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(_targetHooksNoImplicit, ResourceHook.BeforeUpdateRelationship);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock,
                Mock<IResourceHookContainer<Person>> ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, _options);

            // Act
            hookExecutor.BeforeUpdate(_todoList, ResourcePipeline.Patch);

            // Assert
            todoResourceMock.Verify(
                rd => rd.BeforeUpdate(It.Is<IDiffableResourceHashSet<TodoItem>>(diff => TodoCheckDiff(diff, Description)), ResourcePipeline.Patch),
                Times.Once());

            ownerResourceMock.Verify(
                rd => rd.BeforeUpdateRelationship(It.Is<HashSet<string>>(ids => PersonIdCheck(ids, _personId)), It.IsAny<IRelationshipsDictionary<Person>>(),
                    ResourcePipeline.Patch), Times.Once());

            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeUpdate_NoImplicit_Without_Parent_Hook_Implemented()
        {
            // Arrange
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(_targetHooksNoImplicit, ResourceHook.BeforeUpdateRelationship);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock,
                Mock<IResourceHookContainer<Person>> ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, _options);

            // Act
            hookExecutor.BeforeUpdate(_todoList, ResourcePipeline.Patch);

            // Assert
            ownerResourceMock.Verify(
                rd => rd.BeforeUpdateRelationship(It.Is<HashSet<string>>(ids => PersonIdCheck(ids, _personId)),
                    It.Is<IRelationshipsDictionary<Person>>(rh => PersonCheck(LastName, rh)), ResourcePipeline.Patch), Times.Once());

            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeUpdate_NoImplicit_Without_Child_Hook_Implemented()
        {
            // Arrange
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooksNoImplicit, ResourceHook.BeforeUpdate);
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);

            (_, _, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<TodoItem>> todoResourceMock,
                Mock<IResourceHookContainer<Person>> ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, _options);

            // Act
            hookExecutor.BeforeUpdate(_todoList, ResourcePipeline.Patch);

            // Assert
            todoResourceMock.Verify(
                rd => rd.BeforeUpdate(It.Is<IDiffableResourceHashSet<TodoItem>>(diff => TodoCheckDiff(diff, Description)), ResourcePipeline.Patch),
                Times.Once());

            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        private bool TodoCheckDiff(IDiffableResourceHashSet<TodoItem> resources, string checksum)
        {
            ResourceDiffPair<TodoItem> diffPair = resources.GetDiffs().Single();
            bool dbCheck = diffPair.DatabaseValue.Description == checksum;
            bool reqCheck = diffPair.Resource.Description == null;

            KeyValuePair<RelationshipAttribute, HashSet<TodoItem>> updatedRelationship = resources.GetByRelationship<Person>().Single();
            bool diffCheck = updatedRelationship.Key.PublicName == "oneToOnePerson";

            bool getAffectedCheck = resources.GetAffected(todoItem => todoItem.OneToOnePerson).Any();

            return dbCheck && reqCheck && diffCheck && getAffectedCheck;
        }

        private bool TodoCheck(IRelationshipsDictionary<TodoItem> rh, string checksum)
        {
            return rh.GetByRelationship<Person>().Single().Value.First().Description == checksum;
        }

        private bool PersonIdCheck(IEnumerable<string> ids, string checksum)
        {
            return ids.Single() == checksum;
        }

        private bool PersonCheck(string checksum, IRelationshipsDictionary<Person> helper)
        {
            IDictionary<RelationshipAttribute, HashSet<Person>> entries = helper.GetByRelationship<TodoItem>();
            return entries.Single().Value.Single().LastName == checksum;
        }
    }
}
