
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace UnitTests.Models
{
    public class ResourceHooks_Tests
    {


        public ResourceHooks_Tests()
        {
            // Build() exposes the static ResourceGraphBuilder.Instance member, which 
            // is consumed by ResourceDefinition class.
            new ResourceGraphBuilder().Build();

        }

        [Fact]
        public void Hook_Discovery()
        {
            var hookConfig = new ImplementedResourceHooks<Dummy>();
            Assert.Contains(ResourceHook.BeforeDelete, hookConfig.ImplementedHooks);
            Assert.Contains(ResourceHook.AfterDelete, hookConfig.ImplementedHooks);
            Assert.Equal(2, hookConfig.ImplementedHooks.Length);
        }

        [Fact]
        public void BeforeCreate_Hook_Is_Called()
        {
            (var resourceDefinitionMock, var contextMock) = CreateMocks();

            var hookExecutor = new ResourceHookExecutor<DummyWithAllHooks>(contextMock.Object, new ImplementedResourceHooks<DummyWithAllHooks>());
            hookExecutor.BeforeCreate(It.IsAny<DummyWithAllHooks>(), It.IsAny<ResourceAction>());
            resourceDefinitionMock.Verify(rd => rd.BeforeCreate(It.IsAny<DummyWithAllHooks>(), It.IsAny<ResourceAction>()), Times.Once());
        }

        [Fact]
        public void AfterCreate_Hook_Is_Called()
        {
            (var resourceDefinitionMock, var contextMock) = CreateMocks();

            var hookExecutor = new ResourceHookExecutor<DummyWithAllHooks>(contextMock.Object, new ImplementedResourceHooks<DummyWithAllHooks>());
            hookExecutor.AfterCreate(It.IsAny<DummyWithAllHooks>(), It.IsAny<ResourceAction>());
            resourceDefinitionMock.Verify(rd => rd.AfterCreate(It.IsAny<DummyWithAllHooks>(), It.IsAny<ResourceAction>()), Times.Once());
        }
        [Fact]
        public void BeforeRead_Hook_Is_Called()
        {
            (var resourceDefinitionMock, var contextMock) = CreateMocks();

            var hookExecutor = new ResourceHookExecutor<DummyWithAllHooks>(contextMock.Object, new ImplementedResourceHooks<DummyWithAllHooks>());
            hookExecutor.BeforeRead(It.IsAny<ResourceAction>());
            resourceDefinitionMock.Verify(rd => rd.BeforeRead(It.IsAny<ResourceAction>(), It.IsAny<string>()), Times.Once());
        }
        [Fact]
        public void AfterRead_Hook_Is_Called()
        {
            (var resourceDefinitionMock, var contextMock) = CreateMocks();

            var hookExecutor = new ResourceHookExecutor<DummyWithAllHooks>(contextMock.Object, new ImplementedResourceHooks<DummyWithAllHooks>());
            hookExecutor.AfterRead(It.IsAny<IEnumerable<DummyWithAllHooks>>(), It.IsAny<ResourceAction>());
            resourceDefinitionMock.Verify(rd => rd.AfterRead(It.IsAny<IEnumerable<DummyWithAllHooks>>(), It.IsAny<ResourceAction>()), Times.Once());
        }
        [Fact]
        public void BeforeUpdate_Hook_Is_Called()
        {
            (var resourceDefinitionMock, var contextMock) = CreateMocks();

            var hookExecutor = new ResourceHookExecutor<DummyWithAllHooks>(contextMock.Object, new ImplementedResourceHooks<DummyWithAllHooks>());
            hookExecutor.BeforeUpdate(It.IsAny<DummyWithAllHooks>(), It.IsAny<ResourceAction>());
            resourceDefinitionMock.Verify(rd => rd.BeforeUpdate(It.IsAny<DummyWithAllHooks>(), It.IsAny<ResourceAction>()), Times.Once());
        }
        [Fact]
        public void AfterUpdate_Hook_Is_Called()
        {
            (var resourceDefinitionMock, var contextMock) = CreateMocks();

            var hookExecutor = new ResourceHookExecutor<DummyWithAllHooks>(contextMock.Object, new ImplementedResourceHooks<DummyWithAllHooks>());
            hookExecutor.AfterUpdate(It.IsAny<DummyWithAllHooks>(), It.IsAny<ResourceAction>());
            resourceDefinitionMock.Verify(rd => rd.AfterUpdate(It.IsAny<DummyWithAllHooks>(), It.IsAny<ResourceAction>()), Times.Once());
        }
        [Fact]
        public void BeforeDelete_Hook_Is_Called()
        {
            (var resourceDefinitionMock, var contextMock) = CreateMocks();

            var hookExecutor = new ResourceHookExecutor<DummyWithAllHooks>(contextMock.Object, new ImplementedResourceHooks<DummyWithAllHooks>());
            hookExecutor.BeforeDelete(It.IsAny<DummyWithAllHooks>(), It.IsAny<ResourceAction>());
            resourceDefinitionMock.Verify(rd => rd.BeforeDelete(It.IsAny<DummyWithAllHooks>(), It.IsAny<ResourceAction>()), Times.Once());
        }
        [Fact]
        public void AfterDelete_Hook_Is_Called()
        {
            (var resourceDefinitionMock, var contextMock) = CreateMocks();

            var hookExecutor = new ResourceHookExecutor<DummyWithAllHooks>(contextMock.Object, new ImplementedResourceHooks<DummyWithAllHooks>());
            hookExecutor.AfterDelete(It.IsAny<DummyWithAllHooks>(), true, It.IsAny<ResourceAction>());
            resourceDefinitionMock.Verify(rd => rd.AfterDelete(It.IsAny<DummyWithAllHooks>(), true, It.IsAny<ResourceAction>()), Times.Once());
        }
        Mock<DummyWithAllHooksResourceDefinition> CreateResourceDefinitionMock()
        {
            var resourceDefinition = new Mock<DummyWithAllHooksResourceDefinition>();
            resourceDefinition.Setup(rd => rd.BeforeCreate(It.IsAny<DummyWithAllHooks>(), It.IsAny<ResourceAction>()));
            resourceDefinition.Setup(rd => rd.AfterCreate(It.IsAny<DummyWithAllHooks>(), It.IsAny<ResourceAction>()));
            resourceDefinition.Setup(rd => rd.BeforeRead(It.IsAny<ResourceAction>(), null));
            resourceDefinition.Setup(rd => rd.AfterRead(It.IsAny<IEnumerable<DummyWithAllHooks>>(), It.IsAny<ResourceAction>()));
            resourceDefinition.Setup(rd => rd.BeforeUpdate(It.IsAny<DummyWithAllHooks>(), It.IsAny<ResourceAction>()));
            resourceDefinition.Setup(rd => rd.AfterUpdate(It.IsAny<DummyWithAllHooks>(), It.IsAny<ResourceAction>()));
            resourceDefinition.Setup(rd => rd.BeforeDelete(It.IsAny<DummyWithAllHooks>(), It.IsAny<ResourceAction>()));
            resourceDefinition.Setup(rd => rd.AfterDelete(It.IsAny<DummyWithAllHooks>(), It.IsAny<bool>(), It.IsAny<ResourceAction>()));
            return resourceDefinition;
        }

        (Mock<DummyWithAllHooksResourceDefinition>, Mock<IJsonApiContext>) CreateMocks()
        {

            var resourceDefinition = CreateResourceDefinitionMock();

            var processorFactory = new Mock<IGenericProcessorFactory>();
            processorFactory.Setup(c => c.GetProcessor<IResourceDefinition>(It.IsAny<Type>(), It.IsAny<Type>()))
                .Returns((IResourceDefinition)resourceDefinition.Object);
            var context = new Mock<IJsonApiContext>();
            context.Setup(c => c.GenericProcessorFactory).Returns(processorFactory.Object);

            return (resourceDefinition, context);
        }
    }
    public class Dummy : Identifiable
    {

    }

    public class DummyResourceDefinition : ResourceDefinition<Dummy>
    {
        public override void BeforeDelete(Dummy entity, ResourceAction actionSource)
        {
        }
        public override void AfterDelete(Dummy entity, bool succeeded, ResourceAction actionSource)
        {

        }
    }

    public class DummyWithAllHooks : Identifiable
    {

    }

    public class DummyWithAllHooksResourceDefinition : ResourceDefinition<DummyWithAllHooks>
    {
        public override DummyWithAllHooks BeforeCreate(DummyWithAllHooks entity, ResourceAction actionSource)
        {
            return entity;
        }
        public override DummyWithAllHooks AfterCreate(DummyWithAllHooks entity, ResourceAction actionSource)
        {
            return entity;
        }
        public override void BeforeRead(ResourceAction actionSource, string stringId = null)
        {

        }
        public override IEnumerable<DummyWithAllHooks> AfterRead(IEnumerable<DummyWithAllHooks> entities, ResourceAction actionSource)
        {
            return entities;
        }
        public override DummyWithAllHooks BeforeUpdate(DummyWithAllHooks entity, ResourceAction actionSource)
        {
            return entity;
        }
        public override DummyWithAllHooks AfterUpdate(DummyWithAllHooks entity, ResourceAction actionSource)
        {
            return entity;
        }
        public override void BeforeDelete(DummyWithAllHooks entity, ResourceAction actionSource)
        {
        }
        public override void AfterDelete(DummyWithAllHooks entity, bool succeeded, ResourceAction actionSource)
        {

        }
    }
}
