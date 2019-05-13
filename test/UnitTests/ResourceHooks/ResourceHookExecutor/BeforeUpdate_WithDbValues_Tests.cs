using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public class BeforeUpdate_WithDbValues_Tests : ResourceHooksTestBase
    {
        private readonly string description = "DESCRIPTION";
        private readonly string lastName = "NAME";
        private readonly string implicitPersonId;
        private readonly string personId;
        private readonly List<TodoItem> todoList;
        private readonly DbContextOptions<AppDbContext> options;

        public BeforeUpdate_WithDbValues_Tests()
        {
            // Build() exposes the static ResourceGraphBuilder.Instance member, which 
            // is consumed by ResourceDefinition class.
            new ResourceGraphBuilder()
                .AddResource<TodoItem>()
                .AddResource<Person>()
                .Build();

            todoList = CreateTodoWithOwner();

            var todoId = todoList[0].Id;
            var _personId = todoList[0].Owner.Id;
            personId = _personId.ToString();
            var _implicitPersonId = (_personId + 10000);
            implicitPersonId = _implicitPersonId.ToString();
            options = InitInMemoryDb(context =>
            {
                context.Set<Person>().Add(new Person { Id = _personId, LastName = lastName });
                context.Set<Person>().Add(new Person { Id = _implicitPersonId, LastName = lastName + lastName });
                context.Set<TodoItem>().Add(new TodoItem { Id = todoId, OwnerId = _implicitPersonId, Description = description });
                context.SaveChanges();
            });
        }

        [Fact]
        public void BeforeUpdate() // TODO l=3 implicit needs to be tested here too
        {

            var todoDiscovery = SetDiscoverableHooks<TodoItem>(AllHooks, EnableDbValuesEverywhere);
            var personDiscovery = SetDiscoverableHooks<Person>(AllHooks, EnableDbValuesEverywhere);
            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            // act
            hookExecutor.BeforeUpdate(todoList, ResourceAction.Patch);

            // assert
            todoResourceMock.Verify(rd => rd.BeforeUpdate(It.Is<EntityDiff<TodoItem>>((diff) => TodoCheck(diff, description)), ResourceAction.Patch), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(
                It.Is<IEnumerable<string>>(ids => PersonIdCheck(ids, personId)),
                It.IsAny<IUpdatedRelationshipHelper<Person>>(),
                ResourceAction.Patch),
                Times.Once());

            ownerResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(
                It.Is<IUpdatedRelationshipHelper<Person>>(rh => PersonCheck(lastName + lastName, rh)),
                ResourceAction.Patch),
                Times.Once());

            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }


        [Fact]
        public void BeforeUpdate_Deleting_Relationship() // TODO l=3 implicit needs to be tested here too
        {

            var todoDiscovery = SetDiscoverableHooks<TodoItem>(AllHooks, EnableDbValuesEverywhere);
            var personDiscovery = SetDiscoverableHooks<Person>(AllHooks, EnableDbValuesEverywhere);
            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            var attr = ResourceGraph.Instance.GetContextEntity(typeof(TodoItem)).Relationships.Single(r => r.PublicRelationshipName == "owner");
            contextMock.Setup(c => c.RelationshipsToUpdate).Returns(new Dictionary<RelationshipAttribute, object>() { { attr, new object() } });

            // act
            var todoList = new List<TodoItem>() { new TodoItem { Id = this.todoList[0].Id } };
            hookExecutor.BeforeUpdate(todoList, ResourceAction.Patch);


            // assert
            todoResourceMock.Verify(rd => rd.BeforeUpdate(It.Is<EntityDiff<TodoItem>>((diff) => TodoCheck(diff, description)), ResourceAction.Patch), Times.Once());


            ownerResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(
                It.Is<IUpdatedRelationshipHelper<Person>>(rh => PersonCheck(lastName + lastName, rh)),
                ResourceAction.Patch),
                Times.Once());

            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }


        [Fact]
        public void BeforeUpdate_Without_Parent_Hook_Implemented()
        {
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks);
            var personDiscovery = SetDiscoverableHooks<Person>(AllHooks, EnableDbValuesEverywhere);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            // act
            hookExecutor.BeforeUpdate(todoList, ResourceAction.Patch);

            // assert
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(
                It.Is<IEnumerable<string>>(ids => PersonIdCheck(ids, personId)),
                It.IsAny<IUpdatedRelationshipHelper<Person>>(),
                ResourceAction.Patch),
                Times.Once());

            ownerResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(
                It.Is<IUpdatedRelationshipHelper<Person>>(rh => PersonCheck(lastName + lastName, rh)),
                ResourceAction.Patch),
                Times.Once());

            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeUpdate_Without_Child_Hook_Implemented()  // TODO l=3 implicit needs to be tested here too
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(AllHooks, EnableDbValuesEverywhere);
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            // act
            hookExecutor.BeforeUpdate(todoList, ResourceAction.Patch);
        }

        [Fact]
        public void BeforeUpdate_NoImplicit()
        {

            var todoDiscovery = SetDiscoverableHooks<TodoItem>(AllHooksNoImplicit, new ResourceHook[] { ResourceHook.BeforeUpdate });
            var personDiscovery = SetDiscoverableHooks<Person>(AllHooksNoImplicit, new ResourceHook[] { ResourceHook.BeforeUpdateRelationship });
            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            // act
            hookExecutor.BeforeUpdate(todoList, ResourceAction.Patch);

            // assert
            todoResourceMock.Verify(rd => rd.BeforeUpdate(It.Is<EntityDiff<TodoItem>>((diff) => TodoCheck(diff, description)), ResourceAction.Patch), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(
                It.Is<IEnumerable<string>>(ids => PersonIdCheck(ids, personId)),
                It.IsAny<IUpdatedRelationshipHelper<Person>>(),
                ResourceAction.Patch),
                Times.Once());

            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeUpdate_NoImplicit_Without_Parent_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks);
            var personDiscovery = SetDiscoverableHooks<Person>(AllHooksNoImplicit, new ResourceHook[] { ResourceHook.BeforeUpdateRelationship });
            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            // act
            hookExecutor.BeforeUpdate(todoList, ResourceAction.Patch);

            // assert
            ownerResourceMock.Verify(rd => rd.BeforeUpdateRelationship(
                It.Is<IEnumerable<string>>(ids => PersonIdCheck(ids, personId)),
                It.IsAny<IUpdatedRelationshipHelper<Person>>(),
                ResourceAction.Patch),
                Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void BeforeUpdate_NoImplicit_Without_Child_Hook_Implemented()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(AllHooksNoImplicit, new ResourceHook[] { ResourceHook.BeforeUpdate });
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks);

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery, repoDbContextOptions: options);

            // act
            hookExecutor.BeforeUpdate(todoList, ResourceAction.Patch);

            // assert
            todoResourceMock.Verify(rd => rd.BeforeUpdate(It.Is<EntityDiff<TodoItem>>((diff) => TodoCheck(diff, description)), ResourceAction.Patch), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        private bool TodoCheck(EntityDiff<TodoItem> diff, string checksum)
        {
            var dbCheck = diff.DatabaseEntities.Single().Description == checksum;
            var reqCheck = diff.RequestEntities.Single().Description == null;
            return (dbCheck && reqCheck);
        }

        private bool PersonIdCheck(IEnumerable<string> ids, string checksum)
        {
            return ids.Single() == checksum;
        }

        private bool PersonCheck(string checksum, IUpdatedRelationshipHelper<Person> helper)
        {

            var entries = helper.GetEntitiesRelatedWith<TodoItem>();
            return entries.Single().Value.Single().LastName == checksum;
        }
    }
}

