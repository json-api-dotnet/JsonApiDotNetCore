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
    public class BeforeUpdate_WithDbValues_Tests : HooksTestsSetup
    {
        private readonly ResourceHook[] targetHooks = { ResourceHook.BeforeUpdate, ResourceHook.BeforeImplicitUpdateRelationship, ResourceHook.BeforeUpdateRelationship };
        private readonly ResourceHook[] targetHooksNoImplicit = { ResourceHook.BeforeUpdate, ResourceHook.BeforeUpdateRelationship };

        private readonly string description = "DESCRIPTION";
        private readonly string lastName = "NAME";
        private readonly string personId;
        private readonly List<TodoItem> todoList;
        private readonly DbContextOptions<AppDbContext> options;

        public BeforeUpdate_WithDbValues_Tests()
        {
            todoList = CreateTodoWithToOnePerson();

            var todoId = todoList[0].Id;
            var _personId = todoList[0].ToOnePerson.Id;
            personId = _personId.ToString();
            var _implicitPersonId = (_personId + 10000);

            var implicitTodo = _todoFaker.Generate();
            implicitTodo.Id += 1000;
            implicitTodo.ToOnePersonId = _personId;
            implicitTodo.Description = description + description;

            options = InitInMemoryDb(context =>
            {
                context.Set<Person>().Add(new Person { Id = _personId, LastName = lastName });
                context.Set<Person>().Add(new Person { Id = _implicitPersonId, LastName = lastName + lastName });
                context.Set<TodoItem>().Add(new TodoItem { Id = todoId, ToOnePersonId = _implicitPersonId, Description = description });
                context.Set<TodoItem>().Add(implicitTodo);
                context.SaveChanges();
            });
        }

        [Fact]
        public void BeforeUpdate()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, EnableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, EnableDbValues);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            // act
            hookExecutor.BeforeUpdate(todoList, ResourcePipeline.Patch);

            // assert
            todoResourceMock.Verify(rd => rd.BeforeUpdate(It.Is<IDiffableEntityHashSet<TodoItem>>((diff) => TodoCheckDiff(diff, description)), ResourcePipeline.Patch), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(
                It.Is<HashSet<string>>(ids => PersonIdCheck(ids, personId)),
                It.Is<IRelationshipsDictionary<Person>>(rh => PersonCheck(lastName, rh)),
                ResourcePipeline.Patch),
                Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(
                It.Is<IRelationshipsDictionary<Person>>(rh => PersonCheck(lastName + lastName, rh)),
                ResourcePipeline.Patch),
                Times.Once());
            todoResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(
                It.Is<IRelationshipsDictionary<TodoItem>>(rh => TodoCheck(rh, description + description)),
                ResourcePipeline.Patch),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }


        [Fact]
        public void BeforeUpdate_Deleting_Relationship()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, EnableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, EnableDbValues);
            var (_, ufMock, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            ufMock.Setup(c => c.Relationships).Returns(_resourceGraph.GetRelationships((TodoItem t) => t.ToOnePerson));

            // act
            var _todoList = new List<TodoItem>() { new TodoItem { Id = this.todoList[0].Id } };
            hookExecutor.BeforeUpdate(_todoList, ResourcePipeline.Patch);

            // assert
            todoResourceMock.Verify(rd => rd.BeforeUpdate(It.Is<IDiffableEntityHashSet<TodoItem>>((diff) => TodoCheckDiff(diff, description)), ResourcePipeline.Patch), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(
                It.Is<IRelationshipsDictionary<Person>>(rh => PersonCheck(lastName + lastName, rh)),
                ResourcePipeline.Patch),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }


        [Fact]
        public void BeforeUpdate_Without_Parent_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, EnableDbValues);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            // act
            hookExecutor.BeforeUpdate(todoList, ResourcePipeline.Patch);

            // assert
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(
                It.Is<HashSet<string>>(ids => PersonIdCheck(ids, personId)),
                It.Is<IRelationshipsDictionary<Person>>(rh => PersonCheck(lastName, rh)),
                ResourcePipeline.Patch),
                Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(
                It.Is<IRelationshipsDictionary<Person>>(rh => PersonCheck(lastName + lastName, rh)),
                ResourcePipeline.Patch),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeUpdate_Without_Child_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, EnableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            // act
            hookExecutor.BeforeUpdate(todoList, ResourcePipeline.Patch);

            // assert
            todoResourceMock.Verify(rd => rd.BeforeUpdate(It.Is<IDiffableEntityHashSet<TodoItem>>((diff) => TodoCheckDiff(diff, description)), ResourcePipeline.Patch), Times.Once());
            todoResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(
                It.Is<IRelationshipsDictionary<TodoItem>>(rh => TodoCheck(rh, description + description)),
                ResourcePipeline.Patch),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeUpdate_NoImplicit()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooksNoImplicit, ResourceHook.BeforeUpdate);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooksNoImplicit, ResourceHook.BeforeUpdateRelationship);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            // act
            hookExecutor.BeforeUpdate(todoList, ResourcePipeline.Patch);

            // assert
            todoResourceMock.Verify(rd => rd.BeforeUpdate(It.Is<IDiffableEntityHashSet<TodoItem>>((diff) => TodoCheckDiff(diff, description)), ResourcePipeline.Patch), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(
                It.Is<HashSet<string>>(ids => PersonIdCheck(ids, personId)),
                It.IsAny<IRelationshipsDictionary<Person>>(),
                ResourcePipeline.Patch),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeUpdate_NoImplicit_Without_Parent_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooksNoImplicit, ResourceHook.BeforeUpdateRelationship);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            // act
            hookExecutor.BeforeUpdate(todoList, ResourcePipeline.Patch);

            // assert
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(
                It.Is<HashSet<string>>(ids => PersonIdCheck(ids, personId)),
                It.Is<IRelationshipsDictionary<Person>>(rh => PersonCheck(lastName, rh)),
                ResourcePipeline.Patch),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeUpdate_NoImplicit_Without_Child_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooksNoImplicit, ResourceHook.BeforeUpdate);
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);
            var (_, _, hookExecutor, todoResourceMock, ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            // act
            hookExecutor.BeforeUpdate(todoList, ResourcePipeline.Patch);

            // assert
            todoResourceMock.Verify(rd => rd.BeforeUpdate(It.Is<IDiffableEntityHashSet<TodoItem>>((diff) => TodoCheckDiff(diff, description)), ResourcePipeline.Patch), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        private bool TodoCheckDiff(IDiffableEntityHashSet<TodoItem> entities, string checksum)
        {
            var diffPair = entities.GetDiffs().Single();
            var dbCheck = diffPair.DatabaseValue.Description == checksum;
            var reqCheck = diffPair.Entity.Description == null;
            var diffPairCheck = (dbCheck && reqCheck);

            var updatedRelationship = entities.GetByRelationship<Person>().Single();
            var diffcheck = updatedRelationship.Key.PublicRelationshipName == "one-to-one-person";

            var getAffectedCheck = entities.GetAffected(e => e.ToOnePerson).Any();

            return (dbCheck && reqCheck && diffcheck && getAffectedCheck);
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

