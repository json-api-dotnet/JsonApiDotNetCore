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
    public class BeforeDelete_WithDbValues_Tests : HooksTestsSetup
    {
        private readonly DbContextOptions<AppDbContext> options;
        private readonly Person person;
        public BeforeDelete_WithDbValues_Tests()
        {
            person = _personFaker.Generate();
            var todo1 = _todoFaker.Generate();
            var todo2 = _todoFaker.Generate();
            var passport = _passportFaker.Generate();

            person.Passport = passport;
            person.TodoItems = new List<TodoItem> { todo1 };
            person.StakeHolderTodo = todo2;
            options = InitInMemoryDb(context =>
            {
                context.Set<Person>().Add(person);
                context.SaveChanges();
            });
        }

        [Fact]
        public void BeforeDelete()
        {
            // arrange
            var personDiscovery = SetDiscoverableHooks<Person>(AllHooks, EnableDbValuesEverywhere);
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(AllHooks, EnableDbValuesEverywhere);
            var passportDiscovery = SetDiscoverableHooks<Passport>(AllHooks, EnableDbValuesEverywhere);
            (var contextMock, var hookExecutor, var personResourceMock, var todoResourceMock,
                var passportResourceMock) = CreateTestObjects(personDiscovery, todoDiscovery, passportDiscovery, repoDbContextOptions: options);

            var todoList = CreateTodoWithOwner();
            // act
            hookExecutor.BeforeDelete(new List<Person> { person }, ResourceAction.Delete);

            // assert
            personResourceMock.Verify(rd => rd.BeforeDelete(It.IsAny<IEnumerable<Person>>(), It.IsAny<ResourceAction>()), Times.Once());
            todoResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(It.Is<IUpdatedRelationshipHelper<TodoItem>>( rh => CheckImplicitTodos(rh) ), ResourceAction.Delete), Times.Once());
            passportResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(It.Is<IUpdatedRelationshipHelper<Passport>>( rh => CheckImplicitPassports(rh) ), ResourceAction.Delete), Times.Once());
            VerifyNoOtherCalls(personResourceMock, todoResourceMock, passportResourceMock);
        }

        [Fact]
        public void BeforeDelete_No_Parent_Hooks()
        {
            // arrange
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks, NoHooks);
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(AllHooks, EnableDbValuesEverywhere);
            var passportDiscovery = SetDiscoverableHooks<Passport>(AllHooks, EnableDbValuesEverywhere);
            (var contextMock, var hookExecutor, var personResourceMock, var todoResourceMock,
                var passportResourceMock) = CreateTestObjects(personDiscovery, todoDiscovery, passportDiscovery, repoDbContextOptions: options);

            var todoList = CreateTodoWithOwner();
            // act
            hookExecutor.BeforeDelete(new List<Person> { person }, ResourceAction.Delete);

            // assert
            todoResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(It.Is<IUpdatedRelationshipHelper<TodoItem>>(rh => CheckImplicitTodos(rh)), ResourceAction.Delete), Times.Once());
            passportResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(It.Is<IUpdatedRelationshipHelper<Passport>>(rh => CheckImplicitPassports(rh)), ResourceAction.Delete), Times.Once());
            VerifyNoOtherCalls(personResourceMock, todoResourceMock, passportResourceMock);
        }

        [Fact]
        public void BeforeDelete_No_Children_Hooks()
        {
            // arrange
            var personDiscovery = SetDiscoverableHooks<Person>(AllHooks, EnableDbValuesEverywhere);
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks);
            var passportDiscovery = SetDiscoverableHooks<Passport>(NoHooks);
            (var contextMock, var hookExecutor, var personResourceMock, var todoResourceMock,
                var passportResourceMock) = CreateTestObjects(personDiscovery, todoDiscovery, passportDiscovery, repoDbContextOptions: options);

            var todoList = CreateTodoWithOwner();
            // act
            hookExecutor.BeforeDelete(new List<Person> { person }, ResourceAction.Delete);

            // assert
            personResourceMock.Verify(rd => rd.BeforeDelete(It.IsAny<IEnumerable<Person>>(), It.IsAny<ResourceAction>()), Times.Once());
            VerifyNoOtherCalls(personResourceMock, todoResourceMock, passportResourceMock);
        }

        private bool CheckImplicitTodos(IUpdatedRelationshipHelper<TodoItem> rh)
        {
            var todos = rh.GetEntitiesRelatedWith<Person>().ToList();
            return todos.Count == 2;
        }

        private bool CheckImplicitPassports(IUpdatedRelationshipHelper<Passport> rh)
        {
            var passports = rh.GetEntitiesRelatedWith<Person>().Single().Value;
            return passports.Count == 1;
        }
    }
}

