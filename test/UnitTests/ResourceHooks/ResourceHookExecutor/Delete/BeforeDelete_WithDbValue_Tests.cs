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
    public class BeforeDelete_WithDbValues_Tests : HooksTestsSetup
    {
        private readonly ResourceHook[] targetHooks = { ResourceHook.BeforeDelete, ResourceHook.BeforeImplicitUpdateRelationship, ResourceHook.BeforeUpdateRelationship };

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
            person.StakeHolderTodoItem = todo2;
            options = InitInMemoryDb(context =>
            {
                context.Set<Person>().Add(person);
                context.SaveChanges();
            });
        }

        [Fact]
        public void BeforeDelete()
        {
            // Arrange
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, EnableDbValues);
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, EnableDbValues);
            var passportDiscovery = SetDiscoverableHooks<Passport>(targetHooks, EnableDbValues);
            var (_, hookExecutor, personResourceMock, todoResourceMock, passportResourceMock) = CreateTestObjects(personDiscovery, todoDiscovery, passportDiscovery, repoDbContextOptions: options);

            var todoList = CreateTodoWithOwner();
            // Act
            hookExecutor.BeforeDelete(new List<Person> { person }, ResourcePipeline.Delete);

            // Assert
            personResourceMock.Verify(rd => rd.BeforeDelete(It.IsAny<IEntityHashSet<Person>>(), It.IsAny<ResourcePipeline>()), Times.Once());
            todoResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(It.Is<IRelationshipsDictionary<TodoItem>>(rh => CheckImplicitTodos(rh)), ResourcePipeline.Delete), Times.Once());
            passportResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(It.Is<IRelationshipsDictionary<Passport>>(rh => CheckImplicitPassports(rh)), ResourcePipeline.Delete), Times.Once());
            VerifyNoOtherCalls(personResourceMock, todoResourceMock, passportResourceMock);
        }

        [Fact]
        public void BeforeDelete_No_Parent_Hooks()
        {
            // Arrange
            var personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, EnableDbValues);
            var passportDiscovery = SetDiscoverableHooks<Passport>(targetHooks, EnableDbValues);
            var (_, hookExecutor, personResourceMock, todoResourceMock, passportResourceMock) = CreateTestObjects(personDiscovery, todoDiscovery, passportDiscovery, repoDbContextOptions: options);

            var todoList = CreateTodoWithOwner();
            // Act
            hookExecutor.BeforeDelete(new List<Person> { person }, ResourcePipeline.Delete);

            // Assert
            todoResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(It.Is<IRelationshipsDictionary<TodoItem>>(rh => CheckImplicitTodos(rh)), ResourcePipeline.Delete), Times.Once());
            passportResourceMock.Verify(rd => rd.BeforeImplicitUpdateRelationship(It.Is<IRelationshipsDictionary<Passport>>(rh => CheckImplicitPassports(rh)), ResourcePipeline.Delete), Times.Once());
            VerifyNoOtherCalls(personResourceMock, todoResourceMock, passportResourceMock);
        }

        [Fact]
        public void BeforeDelete_No_Children_Hooks()
        {
            // Arrange
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, EnableDbValues);
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            var passportDiscovery = SetDiscoverableHooks<Passport>(NoHooks, DisableDbValues);
            var (_, hookExecutor, personResourceMock, todoResourceMock, passportResourceMock) = CreateTestObjects(personDiscovery, todoDiscovery, passportDiscovery, repoDbContextOptions: options);

            var todoList = CreateTodoWithOwner();
            // Act
            hookExecutor.BeforeDelete(new List<Person> { person }, ResourcePipeline.Delete);

            // Assert
            personResourceMock.Verify(rd => rd.BeforeDelete(It.IsAny<IEntityHashSet<Person>>(), It.IsAny<ResourcePipeline>()), Times.Once());
            VerifyNoOtherCalls(personResourceMock, todoResourceMock, passportResourceMock);
        }

        private bool CheckImplicitTodos(IRelationshipsDictionary<TodoItem> rh)
        {
            var todos = rh.GetByRelationship<Person>().ToList();
            return todos.Count == 2;
        }

        private bool CheckImplicitPassports(IRelationshipsDictionary<Passport> rh)
        {
            var passports = rh.GetByRelationship<Person>().Single().Value;
            return passports.Count == 1;
        }
    }
}

