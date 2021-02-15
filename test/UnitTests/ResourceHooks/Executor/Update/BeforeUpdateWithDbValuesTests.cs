using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace UnitTests.ResourceHooks.Executor.Update
{
    public sealed class BeforeUpdateWithDbValuesTests : HooksTestsSetup
    {
        private readonly ResourceHook[] _targetHooks = { ResourceHook.BeforeUpdate, ResourceHook.BeforeImplicitUpdateRelationship, ResourceHook.BeforeUpdateRelationship };
        private readonly ResourceHook[] _targetHooksNoImplicit = { ResourceHook.BeforeUpdate, ResourceHook.BeforeUpdateRelationship };

        private const string Description = "DESCRIPTION";
        private const string LastName = "NAME";
        private readonly string _personId;
        private readonly List<TodoItem> _todoList;
        private readonly DbContextOptions<AppDbContext> _options;

        public BeforeUpdateWithDbValuesTests()
        {
            _todoList = CreateTodoWithToOnePerson();

            var todoId = _todoList[0].Id;
            var personId = _todoList[0].OneToOnePerson.Id;
            _personId = personId.ToString();
            var implicitPersonId = personId + 10000;

            var implicitTodo = _todoFaker.Generate();
            implicitTodo.Id += 1000;
            implicitTodo.OneToOnePerson = new Person {Id = personId, LastName = LastName};
            implicitTodo.Description = Description + Description;

            _options = InitInMemoryDb(context =>
            {
                context.Set<TodoItem>().Add(new TodoItem {Id = todoId, OneToOnePerson = new Person {Id = implicitPersonId, LastName = LastName + LastName}, Description = Description});
                context.Set<TodoItem>().Add(implicitTodo);
                context.SaveChanges();
            });
        }

        [Fact]
        public void BeforeUpdate()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, EnableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, EnableDbValues);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: _options);

            // Act
            hookExecutor.BeforeUpdate(_todoList, ResourcePipeline.Patch);

            // Assert
            todoResourceMock.Verify(rd => rd.BeforeUpdate(It.Is<IDiffableResourceHashSet<TodoItem>>((diff) => TodoCheckDiff(diff, Description)), ResourcePipeline.Patch), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(
                It.Is<HashSet<string>>(ids => PersonIdCheck(ids, _personId)),
                It.Is<IRelationshipsDictionary<Person>>(rh => PersonCheck(LastName, rh)),
                ResourcePipeline.Patch),
                Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(
                It.Is<IRelationshipsDictionary<Person>>(rh => PersonCheck(LastName + LastName, rh)),
                ResourcePipeline.Patch),
                Times.Once());
            todoResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(
                It.Is<IRelationshipsDictionary<TodoItem>>(rh => TodoCheck(rh, Description + Description)),
                ResourcePipeline.Patch),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }


        [Fact]
        public void BeforeUpdate_Deleting_Relationship()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, EnableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, EnableDbValues);
            var (_, ufMock, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: _options);

            ufMock.Setup(c => c.Relationships).Returns(ResourceGraph.GetRelationships((TodoItem t) => t.OneToOnePerson).ToHashSet);

            // Act
            var todoList = new List<TodoItem> { new TodoItem { Id = _todoList[0].Id } };
            hookExecutor.BeforeUpdate(todoList, ResourcePipeline.Patch);

            // Assert
            todoResourceMock.Verify(rd => rd.BeforeUpdate(It.Is<IDiffableResourceHashSet<TodoItem>>((diff) => TodoCheckDiff(diff, Description)), ResourcePipeline.Patch), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(
                It.Is<IRelationshipsDictionary<Person>>(rh => PersonCheck(LastName + LastName, rh)),
                ResourcePipeline.Patch),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }


        [Fact]
        public void BeforeUpdate_Without_Parent_Hook_Implemented()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, EnableDbValues);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: _options);

            // Act
            hookExecutor.BeforeUpdate(_todoList, ResourcePipeline.Patch);

            // Assert
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(
                It.Is<HashSet<string>>(ids => PersonIdCheck(ids, _personId)),
                It.Is<IRelationshipsDictionary<Person>>(rh => PersonCheck(LastName, rh)),
                ResourcePipeline.Patch),
                Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(
                It.Is<IRelationshipsDictionary<Person>>(rh => PersonCheck(LastName + LastName, rh)),
                ResourcePipeline.Patch),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeUpdate_Without_Child_Hook_Implemented()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, EnableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: _options);

            // Act
            hookExecutor.BeforeUpdate(_todoList, ResourcePipeline.Patch);

            // Assert
            todoResourceMock.Verify(rd => rd.BeforeUpdate(It.Is<IDiffableResourceHashSet<TodoItem>>((diff) => TodoCheckDiff(diff, Description)), ResourcePipeline.Patch), Times.Once());
            todoResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(
                It.Is<IRelationshipsDictionary<TodoItem>>(rh => TodoCheck(rh, Description + Description)),
                ResourcePipeline.Patch),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeUpdate_NoImplicit()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooksNoImplicit, ResourceHook.BeforeUpdate);
            var personDiscovery = SetDiscoverableHooks<Person>(_targetHooksNoImplicit, ResourceHook.BeforeUpdateRelationship);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: _options);

            // Act
            hookExecutor.BeforeUpdate(_todoList, ResourcePipeline.Patch);

            // Assert
            todoResourceMock.Verify(rd => rd.BeforeUpdate(It.Is<IDiffableResourceHashSet<TodoItem>>((diff) => TodoCheckDiff(diff, Description)), ResourcePipeline.Patch), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(
                It.Is<HashSet<string>>(ids => PersonIdCheck(ids, _personId)),
                It.IsAny<IRelationshipsDictionary<Person>>(),
                ResourcePipeline.Patch),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeUpdate_NoImplicit_Without_Parent_Hook_Implemented()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(_targetHooksNoImplicit, ResourceHook.BeforeUpdateRelationship);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: _options);

            // Act
            hookExecutor.BeforeUpdate(_todoList, ResourcePipeline.Patch);

            // Assert
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(
                It.Is<HashSet<string>>(ids => PersonIdCheck(ids, _personId)),
                It.Is<IRelationshipsDictionary<Person>>(rh => PersonCheck(LastName, rh)),
                ResourcePipeline.Patch),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeUpdate_NoImplicit_Without_Child_Hook_Implemented()
        {
            // Arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooksNoImplicit, ResourceHook.BeforeUpdate);
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: _options);

            // Act
            hookExecutor.BeforeUpdate(_todoList, ResourcePipeline.Patch);

            // Assert
            todoResourceMock.Verify(rd => rd.BeforeUpdate(It.Is<IDiffableResourceHashSet<TodoItem>>((diff) => TodoCheckDiff(diff, Description)), ResourcePipeline.Patch), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        private bool TodoCheckDiff(IDiffableResourceHashSet<TodoItem> resources, string checksum)
        {
            var diffPair = resources.GetDiffs().Single();
            var dbCheck = diffPair.DatabaseValue.Description == checksum;
            var reqCheck = diffPair.Resource.Description == null;

            var updatedRelationship = resources.GetByRelationship<Person>().Single();
            var diffCheck = updatedRelationship.Key.PublicName == "oneToOnePerson";

            var getAffectedCheck = resources.GetAffected(e => e.OneToOnePerson).Any();

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
            var entries = helper.GetByRelationship<TodoItem>();
            return entries.Single().Value.Single().LastName == checksum;
        }
    }
}

