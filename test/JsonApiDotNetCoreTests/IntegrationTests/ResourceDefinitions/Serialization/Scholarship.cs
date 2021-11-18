using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Serialization
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Scholarship : Identifiable<int>
    {
        [Attr]
        public string ProgramName { get; set; } = null!;

        [Attr]
        public decimal Amount { get; set; }

        [HasMany]
        public IList<Student> Participants { get; set; } = new List<Student>();

        [HasOne]
        public Student? PrimaryContact { get; set; }
    }
}
