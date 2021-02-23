using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Building;
using JsonApiDotNetCore.Serialization.Objects;
using Moq;

namespace UnitTests.Serialization
{
    public class SerializerTestsSetup : SerializationTestsSetupBase
    {
        protected readonly TopLevelLinks DummyTopLevelLinks;
        protected readonly ResourceLinks DummyResourceLinks;
        protected readonly RelationshipLinks DummyRelationshipLinks;

        public SerializerTestsSetup()
        {
            DummyTopLevelLinks = new TopLevelLinks
            {
                Self = "http://www.dummy.com/dummy-self-link",
                Next = "http://www.dummy.com/dummy-next-link",
                Prev = "http://www.dummy.com/dummy-prev-link",
                First = "http://www.dummy.com/dummy-first-link",
                Last = "http://www.dummy.com/dummy-last-link"
            };

            DummyResourceLinks = new ResourceLinks
            {
                Self = "http://www.dummy.com/dummy-resource-self-link"
            };

            DummyRelationshipLinks = new RelationshipLinks
            {
                Related = "http://www.dummy.com/dummy-relationship-related-link",
                Self = "http://www.dummy.com/dummy-relationship-self-link"
            };
        }

        protected ResponseSerializer<T> GetResponseSerializer<T>(IEnumerable<IEnumerable<RelationshipAttribute>> inclusionChains = null,
            Dictionary<string, object> metaDict = null, TopLevelLinks topLinks = null, ResourceLinks resourceLinks = null,
            RelationshipLinks relationshipLinks = null)
            where T : class, IIdentifiable
        {
            IMetaBuilder meta = GetMetaBuilder(metaDict);
            ILinkBuilder link = GetLinkBuilder(topLinks, resourceLinks, relationshipLinks);
            IEnumerable<IQueryConstraintProvider> includeConstraints = GetIncludeConstraints(inclusionChains);
            IIncludedResourceObjectBuilder includedBuilder = GetIncludedBuilder();
            IFieldsToSerialize fieldsToSerialize = GetSerializableFields();

            var resourceObjectBuilder = new ResponseResourceObjectBuilder(link, includedBuilder, includeConstraints, ResourceGraph,
                GetResourceDefinitionAccessor(), GetSerializerSettingsProvider());

            return new ResponseSerializer<T>(meta, link, includedBuilder, fieldsToSerialize, resourceObjectBuilder, new JsonApiOptions());
        }

        protected ResponseResourceObjectBuilder GetResponseResourceObjectBuilder(IEnumerable<IEnumerable<RelationshipAttribute>> inclusionChains = null,
            ResourceLinks resourceLinks = null, RelationshipLinks relationshipLinks = null)
        {
            ILinkBuilder link = GetLinkBuilder(null, resourceLinks, relationshipLinks);
            IEnumerable<IQueryConstraintProvider> includeConstraints = GetIncludeConstraints(inclusionChains);
            IIncludedResourceObjectBuilder includedBuilder = GetIncludedBuilder();

            return new ResponseResourceObjectBuilder(link, includedBuilder, includeConstraints, ResourceGraph, GetResourceDefinitionAccessor(),
                GetSerializerSettingsProvider());
        }

        private IIncludedResourceObjectBuilder GetIncludedBuilder()
        {
            return new IncludedResourceObjectBuilder(GetSerializableFields(), GetLinkBuilder(), ResourceGraph, Enumerable.Empty<IQueryConstraintProvider>(),
                GetResourceDefinitionAccessor(), GetSerializerSettingsProvider());
        }

        protected IResourceObjectBuilderSettingsProvider GetSerializerSettingsProvider()
        {
            var mock = new Mock<IResourceObjectBuilderSettingsProvider>();
            mock.Setup(m => m.Get()).Returns(new ResourceObjectBuilderSettings());
            return mock.Object;
        }

        protected IResourceDefinitionAccessor GetResourceDefinitionAccessor()
        {
            var mock = new Mock<IResourceDefinitionAccessor>();
            return mock.Object;
        }

        protected IMetaBuilder GetMetaBuilder(Dictionary<string, object> meta = null)
        {
            var mock = new Mock<IMetaBuilder>();
            mock.Setup(m => m.Build()).Returns(meta);
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
            mock.Setup(m => m.GetAttributes(It.IsAny<Type>())).Returns<Type>(t => ResourceGraph.GetResourceContext(t).Attributes);
            mock.Setup(m => m.GetRelationships(It.IsAny<Type>())).Returns<Type>(t => ResourceGraph.GetResourceContext(t).Relationships);
            return mock.Object;
        }

        protected IEnumerable<IQueryConstraintProvider> GetIncludeConstraints(IEnumerable<IEnumerable<RelationshipAttribute>> inclusionChains = null)
        {
            var expressionsInScope = new List<ExpressionInScope>();

            if (inclusionChains != null)
            {
                List<ResourceFieldChainExpression> chains = inclusionChains.Select(relationships => new ResourceFieldChainExpression(relationships.ToArray()))
                    .ToList();

                IncludeExpression includeExpression = IncludeChainConverter.FromRelationshipChains(chains);
                expressionsInScope.Add(new ExpressionInScope(null, includeExpression));
            }

            var mock = new Mock<IQueryConstraintProvider>();
            mock.Setup(x => x.GetConstraints()).Returns(expressionsInScope);

            IQueryConstraintProvider includeConstraintProvider = mock.Object;
            return includeConstraintProvider.AsEnumerable();
        }

        /// <summary>
        /// Minimal implementation of abstract JsonApiSerializer base class, with the purpose of testing the business logic for building the document structure.
        /// </summary>
        protected sealed class TestSerializer : BaseSerializer
        {
            public TestSerializer(IResourceObjectBuilder resourceObjectBuilder)
                : base(resourceObjectBuilder)
            {
            }

            public new Document Build(IIdentifiable resource, IReadOnlyCollection<AttrAttribute> attributes = null,
                IReadOnlyCollection<RelationshipAttribute> relationships = null)
            {
                return base.Build(resource, attributes, relationships);
            }

            public new Document Build(IReadOnlyCollection<IIdentifiable> resources, IReadOnlyCollection<AttrAttribute> attributes = null,
                IReadOnlyCollection<RelationshipAttribute> relationships = null)
            {
                return base.Build(resources, attributes, relationships);
            }
        }
    }
}
