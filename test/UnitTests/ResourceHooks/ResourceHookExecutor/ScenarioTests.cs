using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public class SameEntityTypeTests : HooksTestsSetup
    {
        private readonly ResourceHook[] targetHooks = { ResourceHook.OnReturn };

        [Fact]
        public void Entity_Has_Multiple_Relations_To_Same_Type()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            var personDiscovery = SetDiscoverableHooks<Person>(targetHooks, DisableDbValues);
            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var person1 = new Person();
            var todo = new TodoItem { Owner = person1 };
            var person2 = new Person { AssignedTodoItems = new List<TodoItem>() { todo } };
            todo.Assignee = person2;
            var person3 = new Person { StakeHolderTodo = todo };
            todo.StakeHolders = new List<Person> { person3 };
            var todoList = new List<TodoItem>() { todo };

            // act
            hookExecutor.OnReturn(todoList, ResourceAction.Create);

            // assert
            todoResourceMock.Verify(rd => rd.OnReturn(It.IsAny<IEnumerable<TodoItem>>(), ResourceAction.Create), Times.Once());
            ownerResourceMock.Verify(rd => rd.OnReturn(It.IsAny<IEnumerable<Person>>(), ResourceAction.Create), Times.Once());
            VerifyNoOtherCalls(todoResourceMock, ownerResourceMock);
        }

        [Fact]
        public void Entity_Has_Cyclic_Relations()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            (var contextMock, var hookExecutor, var todoResourceMock) = CreateTestObjects(todoDiscovery);            
            var todo = new TodoItem();
            todo.ParentTodoItem = todo;
            todo.ChildrenTodoItems = new List<TodoItem> { todo };
            var todoList = new List<TodoItem>() { todo };

            // act
            hookExecutor.OnReturn(todoList, ResourceAction.Create);

            // assert
            todoResourceMock.Verify(rd => rd.OnReturn(It.IsAny<IEnumerable<TodoItem>>(), ResourceAction.Create), Times.Once());
            VerifyNoOtherCalls(todoResourceMock);
        }

        [Fact]
        public void Entity_Has_Nested_Cyclic_Relations()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>(targetHooks, DisableDbValues);
            (var contextMock, var hookExecutor, var todoResourceMock) = CreateTestObjects(todoDiscovery);
            var rootTodo = new TodoItem();
            var child = new TodoItem { ParentTodoItem = rootTodo };
            rootTodo.ChildrenTodoItems = new List<TodoItem> { child };
            var grandChild = new TodoItem() { ParentTodoItem = child };
            child.ChildrenTodoItems = new List<TodoItem> { grandChild };
            var greatGrandChild = new TodoItem() { ParentTodoItem = grandChild };
            greatGrandChild.ChildrenTodoItems = new List<TodoItem> { rootTodo }; ;
            var todoList = new List<TodoItem>() { rootTodo };

            // act
            hookExecutor.OnReturn(todoList, ResourceAction.Create);

            // assert
            todoResourceMock.Verify(rd => rd.OnReturn(It.IsAny<IEnumerable<TodoItem>>(), ResourceAction.Create), Times.Once);
            VerifyNoOtherCalls(todoResourceMock);
        }
    }
}

