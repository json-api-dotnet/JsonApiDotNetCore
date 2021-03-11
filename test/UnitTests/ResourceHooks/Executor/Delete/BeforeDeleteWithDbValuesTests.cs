using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Hooks.Internal;
using JsonApiDotNetCore.Hooks.Internal.Discovery;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace UnitTests.ResourceHooks.Executor.Delete
{
    public sealed class BeforeDeleteWithDbValuesTests : HooksTestsSetup
    {
        private readonly ResourceHook[] _targetHooks =
        {
            ResourceHook.BeforeDelete,
            ResourceHook.BeforeImplicitUpdateRelationship,
            ResourceHook.BeforeUpdateRelationship
        };

        private readonly DbContextOptions<AppDbContext> _options;
        private readonly Person _person;

        public BeforeDeleteWithDbValuesTests()
        {
            _person = PersonFaker.Generate();
            TodoItem todo1 = TodoFaker.Generate();
            TodoItem todo2 = TodoFaker.Generate();
            Passport passport = PassportFaker.Generate();

            _person.Passport = passport;

            _person.TodoItems = new HashSet<TodoItem>
            {
                todo1
            };

            _person.StakeHolderTodoItem = todo2;

            _options = InitInMemoryDb(context =>
            {
                context.Set<Person>().Add(_person);
                context.SaveChanges();
            });
        }

        [Fact]
        public void BeforeDelete()
        {
            // Arrange
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, EnableDbValues);
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, EnableDbValues);
            IHooksDiscovery<Passport> passportDiscovery = SetDiscoverableHooks<Passport>(_targetHooks, EnableDbValues);

            (_, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<Person>> personResourceMock,
                    Mock<IResourceHookContainer<TodoItem>> todoResourceMock, Mock<IResourceHookContainer<Passport>> passportResourceMock) =
                CreateTestObjectsC(personDiscovery, todoDiscovery, passportDiscovery, _options);

            // Act
            hookExecutor.BeforeDelete(_person.AsList(), ResourcePipeline.Delete);

            // Assert
            personResourceMock.Verify(rd => rd.BeforeDelete(It.IsAny<IResourceHashSet<Person>>(), It.IsAny<ResourcePipeline>()), Times.Once());

            todoResourceMock.Verify(
                rd => rd.BeforeImplicitUpdateRelationship(It.Is<IRelationshipsDictionary<TodoItem>>(rh => CheckImplicitTodoItems(rh)), ResourcePipeline.Delete),
                Times.Once());

            passportResourceMock.Verify(
                rd => rd.BeforeImplicitUpdateRelationship(It.Is<IRelationshipsDictionary<Passport>>(rh => CheckImplicitPassports(rh)), ResourcePipeline.Delete),
                Times.Once());

            VerifyNoOtherCalls(personResourceMock, todoResourceMock, passportResourceMock);
        }

        [Fact]
        public void BeforeDelete_No_Parent_Hooks()
        {
            // Arrange
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(NoHooks, DisableDbValues);
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(_targetHooks, EnableDbValues);
            IHooksDiscovery<Passport> passportDiscovery = SetDiscoverableHooks<Passport>(_targetHooks, EnableDbValues);

            (_, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<Person>> personResourceMock,
                    Mock<IResourceHookContainer<TodoItem>> todoResourceMock, Mock<IResourceHookContainer<Passport>> passportResourceMock) =
                CreateTestObjectsC(personDiscovery, todoDiscovery, passportDiscovery, _options);

            // Act
            hookExecutor.BeforeDelete(_person.AsList(), ResourcePipeline.Delete);

            // Assert
            todoResourceMock.Verify(
                rd => rd.BeforeImplicitUpdateRelationship(It.Is<IRelationshipsDictionary<TodoItem>>(rh => CheckImplicitTodoItems(rh)), ResourcePipeline.Delete),
                Times.Once());

            passportResourceMock.Verify(
                rd => rd.BeforeImplicitUpdateRelationship(It.Is<IRelationshipsDictionary<Passport>>(rh => CheckImplicitPassports(rh)), ResourcePipeline.Delete),
                Times.Once());

            VerifyNoOtherCalls(personResourceMock, todoResourceMock, passportResourceMock);
        }

        [Fact]
        public void BeforeDelete_No_Children_Hooks()
        {
            // Arrange
            IHooksDiscovery<Person> personDiscovery = SetDiscoverableHooks<Person>(_targetHooks, EnableDbValues);
            IHooksDiscovery<TodoItem> todoDiscovery = SetDiscoverableHooks<TodoItem>(NoHooks, DisableDbValues);
            IHooksDiscovery<Passport> passportDiscovery = SetDiscoverableHooks<Passport>(NoHooks, DisableDbValues);

            (_, IResourceHookExecutor hookExecutor, Mock<IResourceHookContainer<Person>> personResourceMock,
                    Mock<IResourceHookContainer<TodoItem>> todoResourceMock, Mock<IResourceHookContainer<Passport>> passportResourceMock) =
                CreateTestObjectsC(personDiscovery, todoDiscovery, passportDiscovery, _options);

            // Act
            hookExecutor.BeforeDelete(_person.AsList(), ResourcePipeline.Delete);

            // Assert
            personResourceMock.Verify(rd => rd.BeforeDelete(It.IsAny<IResourceHashSet<Person>>(), It.IsAny<ResourcePipeline>()), Times.Once());
            VerifyNoOtherCalls(personResourceMock, todoResourceMock, passportResourceMock);
        }

        private bool CheckImplicitTodoItems(IRelationshipsDictionary<TodoItem> rh)
        {
            IDictionary<RelationshipAttribute, HashSet<TodoItem>> todoItems = rh.GetByRelationship<Person>();
            return todoItems.Count == 2;
        }

        private bool CheckImplicitPassports(IRelationshipsDictionary<Passport> rh)
        {
            HashSet<Passport> passports = rh.GetByRelationship<Person>().Single().Value;
            return passports.Count == 1;
        }
    }
}
