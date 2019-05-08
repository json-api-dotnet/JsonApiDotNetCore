//using JsonApiDotNetCore.Builders;
//using JsonApiDotNetCore.Models;
//using JsonApiDotNetCore.Services;
//using JsonApiDotNetCoreExample.Models;
//using Moq;
//using System.Collections.Generic;
//using Xunit;

//namespace UnitTests.ResourceHooks
//{
//    public class BeforeCreateTests : ResourceHooksTestBase
//    {
//        public BeforeCreateTests()
//        {
//            // Build() exposes the static ResourceGraphBuilder.Instance member, which 
//            // is consumed by ResourceDefinition class.
//            new ResourceGraphBuilder()
//                .AddResource<TodoItem>()
//                .AddResource<Person>()
//                .Build();
//        }

//        [Fact]
//        public void BeforeCreate()
//        {
//            // arrange
//            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
//            var personDiscovery = SetDiscoverableHooks<Person>();

//            (var contextMock, var hookExecutor, var todoResourceMock,
//                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
//            var todoList = CreateTodoWithOwner();

//            // act
//            hookExecutor.BeforeCreate(todoList, ResourceAction.Create);
//            // assert
//            todoResourceMock.Verify(rd => rd.BeforeCreate(It.IsAny<EntityDiff<TodoItem>>(), It.IsAny<HookExecutionContext<TodoItem>>()), Times.Once());
//            ownerResourceMock.Verify(rd => rd.BeforeUpdate(It.IsAny<EntityDiff<Person>>(), It.IsAny<HookExecutionContext<Person>>()), Times.Once());

//            todoResourceMock.VerifyNoOtherCalls();
//            ownerResourceMock.VerifyNoOtherCalls();
//        }

//        [Fact]
//        public void BeforeCreate_Without_Parent_Hook_Implemented()
//        {
//            // arrange
//            var todoDiscovery = SetDiscoverableHooks<TodoItem>(new ResourceHook[0]);
//            var personDiscovery = SetDiscoverableHooks<Person>();

//            (var contextMock, var hookExecutor, var todoResourceMock,
//                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
//            var todoList = CreateTodoWithOwner();

//            // act
//            hookExecutor.BeforeCreate(todoList, ResourceAction.Create);
//            // assert
//            todoResourceMock.Verify(rd => rd.BeforeCreate(It.IsAny<EntityDiff<TodoItem>>(), It.IsAny<HookExecutionContext<TodoItem>>()), Times.Never());
//            ownerResourceMock.Verify(rd => rd.BeforeUpdate(It.IsAny<EntityDiff<Person>>(), It.IsAny<HookExecutionContext<Person>>()), Times.Once());

//            todoResourceMock.VerifyNoOtherCalls();
//            ownerResourceMock.VerifyNoOtherCalls();
//        }
//        [Fact]
//        public void BeforeCreate_Without_Child_Hook_Implemented()
//        {
//            // arrange
//            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
//            var personDiscovery = SetDiscoverableHooks<Person>(new ResourceHook[0]);

//            (var contextMock, var hookExecutor, var todoResourceMock,
//                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
//            var todoList = CreateTodoWithOwner();

//            // act
//            hookExecutor.BeforeCreate(todoList, ResourceAction.Create);
//            // assert
//            todoResourceMock.Verify(rd => rd.BeforeCreate(It.IsAny<EntityDiff<TodoItem>>(), It.IsAny<HookExecutionContext<TodoItem>>()), Times.Once());
//            todoResourceMock.VerifyNoOtherCalls();
//            ownerResourceMock.VerifyNoOtherCalls();
//        }
//        [Fact]
//        public void BeforeCreate_Without_Any_Hook_Implemented()
//        {
//            // arrange
//            var todoDiscovery = SetDiscoverableHooks<TodoItem>(new ResourceHook[0]);
//            var personDiscovery = SetDiscoverableHooks<Person>(new ResourceHook[0]);

//            (var contextMock, var hookExecutor, var todoResourceMock,
//                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
//            var todoList = CreateTodoWithOwner();

//            // act
//            hookExecutor.BeforeCreate(todoList, ResourceAction.Create);
//            // assert
//            todoResourceMock.VerifyNoOtherCalls();
//            ownerResourceMock.VerifyNoOtherCalls();
//        }
//    }
//}

