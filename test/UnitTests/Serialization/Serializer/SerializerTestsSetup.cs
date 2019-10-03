using System;
using System.Collections;
using System.Collections.Generic;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Links;
using JsonApiDotNetCore.QueryServices.Contracts;
using JsonApiDotNetCore.Serialization.Serializer;
using JsonApiDotNetCore.Serialization.Serializer.Contracts;
using JsonApiDotNetCore.Services;
using Moq;

namespace UnitTests.Serialization.Serializer
{
    public class SerializerTestsSetup : SerializationTestsSetupBase
    {
        protected readonly IFieldsExplorer _fieldExplorer;
        protected readonly TopLevelLinks _dummyToplevelLinks;
        protected readonly ResourceLinks _dummyResourceLinks;
        protected readonly RelationshipLinks _dummyRelationshipLinks;
        public SerializerTestsSetup()
        {
            _fieldExplorer = new FieldsExplorer(_resourceGraph);
            _dummyToplevelLinks = new TopLevelLinks
            {
                Self = "http://www.dummy.com/dummy-self-link",
                Next = "http://www.dummy.com/dummy-next-link",
                Prev = "http://www.dummy.com/dummy-prev-link",
                First = "http://www.dummy.com/dummy-first-link",
                Last = "http://www.dummy.com/dummy-last-link"
            };
            _dummyResourceLinks = new ResourceLinks
            {
                Self = "http://www.dummy.com/dummy-resource-self-link"
            };
            _dummyRelationshipLinks = new RelationshipLinks
            {
                Related = "http://www.dummy.com/dummy-relationship-related-link",
                Self = "http://www.dummy.com/dummy-relationship-self-link"
            };
        }

        protected ServerSerializer<T> GetServerSerializer<T>(List<List<RelationshipAttribute>> inclusionChains = null, Dictionary<string, object> metaDict = null, TopLevelLinks topLinks = null, ResourceLinks resourceLinks = null, RelationshipLinks relationshipLinks = null) where T : class, IIdentifiable
        {
            var meta = GetMetaBuilder<T>(metaDict);
            var link = GetLinkBuilder(topLinks, resourceLinks, relationshipLinks);
            var fieldsToSerialize = GetSerializableFields();
            var included = GetIncludedRelationships(inclusionChains);
            var provider = GetContextEntityProvider();
            var includedBuilder = GetIncludedBuilder();

            return new ServerSerializer<T>(meta, link, includedBuilder, fieldsToSerialize, included, _resourceGraph, provider, GetSerializerSettingsProvider());
        }

        private IIncludedResourceObjectBuilder GetIncludedBuilder()
        {
            return new IncludedResourceObjectBuilder(GetSerializableFields(), GetLinkBuilder(), _resourceGraph, _resourceGraph, GetSerializerSettingsProvider()) ;
        }

        protected ISerializerSettingsProvider GetSerializerSettingsProvider()
        {
            var mock = new Mock<ISerializerSettingsProvider>();
            return mock.Object;
        }

        private IContextEntityProvider GetContextEntityProvider()
        {
            return _resourceGraph;
        }

        protected IMetaBuilder<T> GetMetaBuilder<T>(Dictionary<string, object> meta = null) where T : class, IIdentifiable
        {
            var mock = new Mock<IMetaBuilder<T>>();
            mock.Setup(m => m.GetMeta()).Returns(meta);
            return mock.Object;
        }

        protected ICurrentRequest GetRequestManager<T>() where T : class, IIdentifiable
        {
            var mock = new Mock<ICurrentRequest>();
            mock.Setup(m => m.GetRequestResource()).Returns(_resourceGraph.GetContextEntity<T>());
            return mock.Object;
        }

        protected ILinkBuilder GetLinkBuilder(TopLevelLinks top = null, ResourceLinks resource = null, RelationshipLinks relationship = null)
        {
            var mock = new Mock<ILinkBuilder>();
            mock.Setup(m => m.GetTopLevelLinks()).Returns(top);
            mock.Setup(m => m.GetResourceLinks(It.IsAny<string>(), It.IsAny<string>())).Returns(resource);
            mock.Setup(m => m.GetRelationshipLinks(It.IsAny<RelationshipAttribute>(), It.IsAny<IIdentifiable>())).Returns(relationship);
            return mock.Object;
        }

        protected IFieldsQueryService GetFieldsQuery()
        {
            var mock = new Mock<IFieldsQueryService>();
            return mock.Object;
        }

        protected IFieldsToSerialize GetSerializableFields()
        {
            var mock = new Mock<IFieldsToSerialize>();
            mock.Setup(m => m.GetAllowedAttributes(It.IsAny<Type>(), null)).Returns<Type>(t => _resourceGraph.GetContextEntity(t).Attributes);
            mock.Setup(m => m.GetAllowedRelationships(It.IsAny<Type>())).Returns<Type>(t => _resourceGraph.GetContextEntity(t).Relationships);
            return mock.Object;
        }

        protected IIncludedQueryService GetIncludedRelationships(List<List<RelationshipAttribute>> inclusionChains = null)
        {
            var mock = new Mock<IIncludedQueryService>();
            if (inclusionChains != null)
                mock.Setup(m => m.Get()).Returns(inclusionChains);

            return mock.Object;
        }


        /// <summary>
        /// Minimal implementation of abstract JsonApiSerializer base class, with
        /// the purpose of testing the business logic for building the document structure.
        /// </summary>
        protected class TestSerializer : DocumentBuilder
        {
            public TestSerializer(IResourceGraph resourceGraph, IContextEntityProvider provider) : base(resourceGraph, provider, null) { }

            public new Document Build(IIdentifiable entity, List<AttrAttribute> attributes = null, List<RelationshipAttribute> relationships = null)
            {
                return base.Build(entity, attributes, relationships);
            }

            public new Document Build(IEnumerable entities, List<AttrAttribute> attributes = null, List<RelationshipAttribute> relationships = null)
            {
                return base.Build(entities, attributes, relationships);
            }
        }
    }
}