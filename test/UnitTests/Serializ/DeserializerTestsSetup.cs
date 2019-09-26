using JsonApiDotNetCore.Models;
using System.Collections.Generic;
using System;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Serialization;

namespace UnitTests.Deserialization
{
    public class DeserializerTestsSetup : SerializationTestBase
    {
        protected Document CreateDocumentWithRelationships(string mainType, string relationshipMemberName, string relatedType = null, bool isToManyData = false)
        {
            var content = CreateDocumentWithRelationships(mainType);
            content.Data.Relationships.Add(relationshipMemberName, CreateRelationshipData(relatedType, isToManyData));
            return content;
        }

        protected Document CreateDocumentWithRelationships(string mainType)
        {
            return new Document
            {
                Data = new ResourceObject
                {
                    Id = "1",
                    Type = mainType,
                    Relationships = new Dictionary<string, RelationshipData> { }
                }
            };
        }

        protected RelationshipData CreateRelationshipData(string relatedType = null, bool isToManyData = false)
        {
            var data = new RelationshipData();
            var rio = relatedType == null ? null : new ResourceIdentifierObject { Id = "10", Type = relatedType };

            if (isToManyData)
            {
                data.ExposedData = new List<ResourceIdentifierObject>();
                if (relatedType != null) ((List<ResourceIdentifierObject>)data.ExposedData).Add(rio);
            } else
            {
                data.ExposedData = rio;
            }
            return data;
        }

        protected Document CreateTestResourceDocument()
        {
            return new Document
            {
                Data = new ResourceObject
                {
                    Type = "test-resource",
                    Id = "1",
                    Attributes = new Dictionary<string, object>
                    {
                        { "string-field", "some string" },
                        { "int-field", 1 },
                        { "nullable-int-field", null },
                        { "guid-field", "1a68be43-cc84-4924-a421-7f4d614b7781" },
                        { "date-time-field", "9/11/2019 11:41:40 AM" }
                    }
                }
            };
        }
    }
}
