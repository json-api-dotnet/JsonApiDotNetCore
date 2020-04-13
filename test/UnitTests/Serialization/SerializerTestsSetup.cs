using System;
using System.Collections;
using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Graph;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Links;
using JsonApiDotNetCore.Query;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Server;
using JsonApiDotNetCore.Serialization.Server.Builders;
using Moq;

namespace UnitTests.Serialization
{
    public class SerializerTestsSetup : SerializationTestsSetupBase
    {
        protected readonly TopLevelLinks _dummyTopLevelLinks;
        protected readonly ResourceLinks _dummyResourceLinks;
        protected readonly RelationshipLinks _dummyRelationshipLinks;
        public SerializerTestsSetup()
        {
            _dummyTopLevelLinks = new TopLevelLinks
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

        protected ResponseSerializer<T> GetResponseSerializer<T>(List<List<RelationshipAttribute>> inclusionChains = null, Dictionary<string, object> metaDict = null, TopLevelLinks topLinks = null, ResourceLinks resourceLinks = null, RelationshipLinks relationshipLinks = null) where T : class, IIdentifiable
        {
            var meta = GetMetaBuilder<T>(metaDict);
            var link = GetLinkBuilder(topLinks, resourceLinks, relationshipLinks);
            var included = GetIncludedRelationships(inclusionChains);
            var includedBuilder = GetIncludedBuilder();
            var fieldsToSerialize = GetSerializableFields();
            ResponseResourceObjectBuilder resourceObjectBuilder = new ResponseResourceObjectBuilder(link, includedBuilder, included, _resourceGraph, GetSerializerSettingsProvider());
            return new ResponseSerializer<T>(meta, link, includedBuilder, fieldsToSerialize, resourceObjectBuilder, new JsonApiOptions());
        }

        protected ResponseResourceObjectBuilder GetResponseResourceObjectBuilder(List<List<RelationshipAttribute>> inclusionChains = null, ResourceLinks resourceLinks = null, RelationshipLinks relationshipLinks = null) 
        {
            var link = GetLinkBuilder(null, resourceLinks, relationshipLinks);
            var included = GetIncludedRelationships(inclusionChains);
            var includedBuilder = GetIncludedBuilder();
            return new ResponseResourceObjectBuilder(link, includedBuilder, included, _resourceGraph, GetSerializerSettingsProvider());
        }

        private IIncludedResourceObjectBuilder GetIncludedBuilder()
        {
            return new IncludedResourceObjectBuilder(GetSerializableFields(), GetLinkBuilder(), _resourceGraph, GetSerializerSettingsProvider());
        }

        protected IResourceObjectBuilderSettingsProvider GetSerializerSettingsProvider()
        {
            var mock = new Mock<IResourceObjectBuilderSettingsProvider>();
            mock.Setup(m => m.Get()).Returns(new ResourceObjectBuilderSettings());
            return mock.Object;
        }

        protected IMetaBuilder<T> GetMetaBuilder<T>(Dictionary<string, object> meta = null) where T : class, IIdentifiable
        {
            var mock = new Mock<IMetaBuilder<T>>();
            mock.Setup(m => m.GetMeta()).Returns(meta);
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

        protected IFieldsToSerialize GetSerializableFields()
        {
            var mock = new Mock<IFieldsToSerialize>();
            mock.Setup(m => m.GetAllowedAttributes(It.IsAny<Type>(), It.IsAny<RelationshipAttribute>())).Returns<Type, RelationshipAttribute>((t, r) => _resourceGraph.GetResourceContext(t).Attributes);
            mock.Setup(m => m.GetAllowedRelationships(It.IsAny<Type>())).Returns<Type>(t => _resourceGraph.GetResourceContext(t).Relationships);
            return mock.Object;
        }

        protected IIncludeService GetIncludedRelationships(List<List<RelationshipAttribute>> inclusionChains = null)
        {
            var mock = new Mock<IIncludeService>();
            if (inclusionChains != null)
                mock.Setup(m => m.Get()).Returns(inclusionChains);

            return mock.Object;
        }

        /// <summary>
        /// Minimal implementation of abstract JsonApiSerializer base class, with
        /// the purpose of testing the business logic for building the document structure.
        /// </summary>
        protected sealed class TestDocumentBuilder : BaseDocumentBuilder
        {
            public TestDocumentBuilder(IResourceObjectBuilder resourceObjectBuilder) : base(resourceObjectBuilder) { }

            public new Document Build(IIdentifiable entity, IReadOnlyCollection<AttrAttribute> attributes = null, IReadOnlyCollection<RelationshipAttribute> relationships = null)
            {
                return base.Build(entity, attributes, relationships);
            }

            public new Document Build(IEnumerable entities, IReadOnlyCollection<AttrAttribute> attributes = null, IReadOnlyCollection<RelationshipAttribute> relationships = null)
            {
                return base.Build(entities, attributes, relationships);
            }
        }
    }
}