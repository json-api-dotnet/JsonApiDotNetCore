using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    public class TestResourceWithAbstractRelationship : Identifiable
    {
        [HasOne] public BaseModel ToOne { get; set; }
        [HasMany] public List<BaseModel> ToMany { get; set; }
    }
}