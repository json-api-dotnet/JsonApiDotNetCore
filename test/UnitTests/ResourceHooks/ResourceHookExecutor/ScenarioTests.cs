using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UnitTests.ResourceHooks
{
    public class ScenarioTests : HooksTestsSetup
    {
        //[Fact]
        //public void Entity_Has_Multiple_Relations_To_Same_Type()
        //{
        //    // arrange
        //    var todoDiscovery = SetDiscoverableHooks<TodoItem>(AllHooksNoImplicit, NoHooks);
        //    var personDiscovery = SetDiscoverableHooks<Person>(AllHooksNoImplicit, NoHooks);

        //    (var contextMock, var hookExecutor, var todoResourceMock,
        //        var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
        //    var person1 = new Person();
        //    var todo = new TodoItem { Owner = person1 };
        //    var person2 = new Person { AssignedTodoItems = new List<TodoItem>() { todo } };
        //    todo.Assignee = person2;
        //    var person3 = new Person { StakeHolderTodo = todo };
        //    todo.StakeHolders = new List<Person> { person3 };

        //    var todoList = new List<TodoItem>() { todo };
            
        //    // act
        //    hookExecutor.AfterCreate(todoList, ResourceAction.Create);
        //    // assert
        //    todoResourceMock.Verify(rd => rd.AfterCreate(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<HookExecutionContext<TodoItem>>()), Times.Once());
        //    ownerResourceMock.Verify(rd => rd.AfterUpdate(It.IsAny<IEnumerable<Person>>(), It.IsAny<HookExecutionContext<Person>>()), Times.Once());

        //    todoResourceMock.VerifyNoOtherCalls();
        //    ownerResourceMock.VerifyNoOtherCalls();
        //}

        //[Fact]
        //public void Entity_Has_Cyclic_Relations()
        //{
        //    // arrange
        //    var todoDiscovery = SetDiscoverableHooks<TodoItem>(AllHooksNoImplicit, NoHooks);

        //    (var contextMock, var hookExecutor, var todoResourceMock) = CreateTestObjects(todoDiscovery);            
        //    var todo = new TodoItem();
        //    todo.ParentTodoItem = todo;
        //    todo.ChildrenTodoItems = new List<TodoItem> { todo };
        //    var todoList = new List<TodoItem>() { todo };
        //    // act
        //    hookExecutor.AfterCreate(todoList, ResourceAction.Create);
        //    // assert
        //    todoResourceMock.Verify(rd => rd.AfterCreate(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<HookExecutionContext<TodoItem>>()), Times.Once());
        //    todoResourceMock.VerifyNoOtherCalls();
        //}

        //[Fact]
        //public void Entity_Has_Nested_Cyclic_Relations()
        //{
        //    // arrange
        //    var todoDiscovery = SetDiscoverableHooks<TodoItem>(AllHooksNoImplicit, NoHooks);

        //    (var contextMock, var hookExecutor, var todoResourceMock) = CreateTestObjects(todoDiscovery);
        //    var rootTodo = new TodoItem();
        //    var child = new TodoItem { ParentTodoItem = rootTodo };
        //    rootTodo.ChildrenTodoItems = new List<TodoItem> { child };
        //    var grandChild = new TodoItem() { ParentTodoItem = child };
        //    child.ChildrenTodoItems = new List<TodoItem> { grandChild };
        //    var greatGrandChild = new TodoItem() { ParentTodoItem = grandChild };
        //    greatGrandChild.ChildrenTodoItems = new List<TodoItem> { rootTodo }; ;

        //    var todoList = new List<TodoItem>() { rootTodo };
        //    // act
        //    hookExecutor.AfterCreate(todoList, ResourceAction.Create);
        //    // assert
        //    todoResourceMock.Verify(rd => rd.AfterCreate(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<HookExecutionContext<TodoItem>>()), Times.Once());
        //    todoResourceMock.Verify(rd => rd.AfterUpdate(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<HookExecutionContext<TodoItem>>()), Times.Exactly(2));
        //    todoResourceMock.VerifyNoOtherCalls();
        //}



    }
}

