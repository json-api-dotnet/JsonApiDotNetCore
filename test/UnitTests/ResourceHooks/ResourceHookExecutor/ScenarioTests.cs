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
    public class ScenarioTests : ResourceHooksTestBase
    {
        readonly IResourceGraph _graph;
        public ScenarioTests()
        {
            // Build() exposes the static ResourceGraphBuilder.Instance member, which 
            // is consumed by ResourceDefinition class.
            _graph = new ResourceGraphBuilder()
                .AddResource<TodoItem>()
                .AddResource<Person>()
                .Build();
        }

        [Fact]
        public void Entity_Has_Multiple_Relations_To_Same_Type()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
            var personDiscovery = SetDiscoverableHooks<Person>();

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
            hookExecutor.AfterCreate(It.IsAny<IEnumerable<TodoItem>>(), ResourceAction.Create);
            // assert
            todoResourceMock.Verify(rd => rd.AfterCreate(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<HookExecutionContext<TodoItem>>()), Times.Once());
            ownerResourceMock.Verify(rd => rd.AfterUpdate(It.IsAny<IEnumerable<Person>>(), It.IsAny<HookExecutionContext<Person>>()), Times.Once());

            todoResourceMock.VerifyNoOtherCalls();
            ownerResourceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void Entity_Has_Cyclic_Relations()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>();

            (var contextMock, var hookExecutor, var todoResourceMock) = CreateTestObjects(todoDiscovery);            
            var todo = new TodoItem();
            todo.ParentTodoItem = todo;
            todo.ChildrenTodoItems = new List<TodoItem> { todo };
            var todoList = new List<TodoItem>() { todo };
            // act
            hookExecutor.AfterCreate(It.IsAny<IEnumerable<TodoItem>>(), ResourceAction.Create);
            // assert
            todoResourceMock.Verify(rd => rd.AfterCreate(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<HookExecutionContext<TodoItem>>()), Times.Once());
            todoResourceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void Entity_Has_Nested_Cyclic_Relations()
        {
            // arrange
            var todoDiscovery = SetDiscoverableHooks<TodoItem>();

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
            hookExecutor.AfterCreate(It.IsAny<IEnumerable<TodoItem>>(), ResourceAction.Create);
            // assert
            todoResourceMock.Verify(rd => rd.AfterCreate(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<HookExecutionContext<TodoItem>>()), Times.Once());
            todoResourceMock.Verify(rd => rd.AfterUpdate(It.IsAny<IEnumerable<TodoItem>>(), It.IsAny<HookExecutionContext<TodoItem>>()), Times.Exactly(2));
            todoResourceMock.VerifyNoOtherCalls();
        }


        [Fact]
        public void Fires_Nested_Hooks_When_Setting_Relationship_To_Null()
        {
            // arrange
            var attr = _graph.GetContextEntity(typeof(TodoItem)).Relationships.Single(r => r.PublicRelationshipName == "owner");


            var todoDiscovery = SetDiscoverableHooks<TodoItem>();
            var personDiscovery = SetDiscoverableHooks<Person>();

            (var contextMock, var hookExecutor, var todoResourceMock,
                var ownerResourceMock) = CreateTestObjects(todoDiscovery, personDiscovery);
            var todoList = CreateTodoWithOwner();



            /// this represents the datastructure that would be received from the 
            /// request body when removing a to-one relation. Note that an assigned
            /// null cannot be distinguished from the default value as a result
            /// of instantiation. We need to rely on JsonApiContext to be able to
            /// recognise this. This is what we're testing here.
            todoList.First().Owner = null;
            contextMock.Setup(c => c.RelationshipsToUpdate)
                    .Returns(new Dictionary<RelationshipAttribute, object>() { { attr, new object() } });


            // act
            hookExecutor.BeforeUpdate(It.IsAny<IEnumerable<TodoItem>>(), ResourceAction.Patch);
            // assert
            todoResourceMock.Verify(rd => rd.BeforeUpdate(It.IsAny<EntityDiff<TodoItem>>(), It.IsAny<HookExecutionContext<TodoItem>>()), Times.Once());
            ownerResourceMock.Verify(rd => rd.BeforeUpdate(It.IsAny<EntityDiff<Person>>(), It.IsAny<HookExecutionContext<Person>>()), Times.Once());

            todoResourceMock.VerifyNoOtherCalls();
            ownerResourceMock.VerifyNoOtherCalls();
        }
    }
}

