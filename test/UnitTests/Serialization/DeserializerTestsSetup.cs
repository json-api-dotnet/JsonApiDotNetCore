using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.JsonConverters;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Http;
using Moq;

namespace UnitTests.Serialization
{
    public class DeserializerTestsSetup : SerializationTestsSetupBase
    {
        protected readonly JsonApiOptions Options = new();
        protected readonly JsonSerializerOptions SerializerWriteOptions;

        protected Mock<IHttpContextAccessor> MockHttpContextAccessor { get; }

        protected DeserializerTestsSetup()
        {
            Options.SerializerOptions.Converters.Add(new ResourceObjectConverter(ResourceGraph));

            SerializerWriteOptions = ((IJsonApiOptions)Options).SerializerWriteOptions;
            MockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            MockHttpContextAccessor.Setup(mock => mock.HttpContext).Returns(new DefaultHttpContext());
        }

        protected Document CreateDocumentWithRelationships(string primaryType, string relationshipMemberName, string relatedType = null,
            bool isToManyData = false)
        {
            Document content = CreateDocumentWithRelationships(primaryType);
            content.SingleData.Relationships.Add(relationshipMemberName, CreateRelationshipData(relatedType, isToManyData));
            return content;
        }

        protected Document CreateDocumentWithRelationships(string primaryType)
        {
            return new()
            {
                Data = new SingleOrManyData<ResourceObject>(new ResourceObject
                {
                    Id = "1",
                    Type = primaryType,
                    Relationships = new Dictionary<string, RelationshipObject>()
                })
            };
        }

        protected RelationshipObject CreateRelationshipData(string relatedType = null, bool isToManyData = false, string id = "10")
        {
            var relationshipObject = new RelationshipObject();

            ResourceIdentifierObject rio = relatedType == null
                ? null
                : new ResourceIdentifierObject
                {
                    Id = id,
                    Type = relatedType
                };

            if (isToManyData)
            {
                IList<ResourceIdentifierObject> rios = relatedType != null ? rio.AsList() : Array.Empty<ResourceIdentifierObject>();
                relationshipObject.Data = new SingleOrManyData<ResourceIdentifierObject>(rios);
            }
            else
            {
                relationshipObject.Data = new SingleOrManyData<ResourceIdentifierObject>(rio);
            }

            return relationshipObject;
        }

        protected Document CreateTestResourceDocument()
        {
            return new()
            {
                Data = new SingleOrManyData<ResourceObject>(new ResourceObject
                {
                    Type = "testResource",
                    Id = "1",
                    Attributes = new Dictionary<string, object>
                    {
                        ["stringField"] = "some string",
                        ["intField"] = 1,
                        ["nullableIntField"] = null,
                        ["guidField"] = "1a68be43-cc84-4924-a421-7f4d614b7781",
                        ["dateTimeField"] = DateTime.Parse("9/11/2019 11:41:40 AM", CultureInfo.InvariantCulture)
                    }
                })
            };
        }

        protected sealed class TestDeserializer : BaseDeserializer
        {
            private readonly IJsonApiOptions _options;

            public TestDeserializer(IResourceGraph resourceGraph, IResourceFactory resourceFactory, IJsonApiOptions options)
                : base(resourceGraph, resourceFactory)
            {
                _options = options;
            }

            public object Deserialize(string body)
            {
                return DeserializeData(body, _options.SerializerReadOptions);
            }

            protected override void AfterProcessField(IIdentifiable resource, ResourceFieldAttribute field, RelationshipObject data = null)
            {
            }
        }
    }
}
