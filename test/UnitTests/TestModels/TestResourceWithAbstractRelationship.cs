using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class TestResourceWithAbstractRelationship : Identifiable
    {
        [HasOne]
        public BaseModel ToOne { get; set; }

        [HasMany]
        public List<BaseModel> ToMany { get; set; }
    }
}
