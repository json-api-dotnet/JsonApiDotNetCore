using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public class BeforeCreate_WithDbValues_Tests : HooksTestsSetup
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
            var _personId = todoList[0].ToOnePerson.Id;
            personId = _personId.ToString();
            var implicitTodo = _todoFaker.Generate();
            implicitTodo.Id += 1000;
            implicitTodo.ToOnePersonId = _personId;
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
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, EnableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, EnableDbValues);
            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            // act
            hookExecutor.BeforeCreate(todoList, ResourceAction.Create);

            // assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(It.Is<HashSet<TodoItem>>((entities) => TodoCheck(entities, description)), ResourceAction.Create), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(
                It.Is<HashSet<string>>(ids => PersonIdCheck(ids, personId)),
                It.IsAny<IUpdatedRelationshipHelper<Person>>(),
                ResourceAction.Create),
                Times.Once());
            todoResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(
                It.Is<IUpdatedRelationshipHelper<TodoItem>>(rh => TodoCheck(rh, description + description)),
                ResourceAction.Create),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_Without_Parent_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, EnableDbValues);
            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            // act
            hookExecutor.BeforeCreate(todoList, ResourceAction.Create);

            // assert
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(
                It.Is<HashSet<string>>(ids => PersonIdCheck(ids, personId)),
                It.IsAny<IUpdatedRelationshipHelper<Person>>(),
                ResourceAction.Create),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_Without_Child_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, EnableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);
            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            // act
            hookExecutor.BeforeCreate(todoList, ResourceAction.Create);

            // assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(It.Is<HashSet<TodoItem>>((entities) => TodoCheck(entities, description)), ResourceAction.Create), Times.Once());
            todoResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(
                It.Is<IUpdatedRelationshipHelper<TodoItem>>(rh => TodoCheck(rh, description + description)),
                ResourceAction.Create),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_NoImplicit()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooksNoImplicit, ResourceHook.BeforeUpdate);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooksNoImplicit, ResourceHook.BeforeUpdateRelationship);
            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            // act
            hookExecutor.BeforeCreate(todoList, ResourceAction.Create);

            // assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(It.Is<HashSet<TodoItem>>((entities) => TodoCheck(entities, description)), ResourceAction.Create), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(
                It.Is<HashSet<string>>(ids => PersonIdCheck(ids, personId)),
                It.IsAny<IUpdatedRelationshipHelper<Person>>(),
                ResourceAction.Create),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_NoImplicit_Without_Parent_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooksNoImplicit, ResourceHook.BeforeUpdateRelationship);
            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            // act
            hookExecutor.BeforeCreate(todoList, ResourceAction.Create);

            // assert
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(
                It.Is<HashSet<string>>(ids => PersonIdCheck(ids, personId)),
                It.IsAny<IUpdatedRelationshipHelper<Person>>(),
                ResourceAction.Create),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeCreate_NoImplicit_Without_Child_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooksNoImplicit, ResourceHook.BeforeUpdate);
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);
            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            // act
            hookExecutor.BeforeCreate(todoList, ResourceAction.Create);

            // assert
            todoResourceMock.Verify(rd => rd.BeforeCreate(It.Is<HashSet<TodoItem>>((entities) => TodoCheck(entities, description)), ResourceAction.Create), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        private bool TodoCheck(IEnumerable<TodoItem> entities, string checksum)
        {
            return entities.Single().Description == checksum;
        }

        private bool TodoCheck(IUpdatedRelationshipHelper<TodoItem> rh, string checksum)
        {
            return rh.EntitiesRelatedTo<Person>().Single().Value.First().Description == checksum;
        }

        private bool PersonIdCheck(IEnumerable<string> ids, string checksum)
        {
            return ids.Single() == checksum;
        }

        private bool PersonCheck(string checksum, IUpdatedRelationshipHelper<Person> helper)
        {

            var entries = helper.EntitiesRelatedTo<TodoItem>();
            return entries.Single().Value.Single().LastName == checksum;
        }
    }
}

