using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations.Meta
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class TextLanguageMetaDefinition : JsonApiResourceDefinition<TextLanguage, Guid>
    {
        internal const string NoticeText = "See https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes for ISO 639-1 language codes.";

        public TextLanguageMetaDefinition(IResourceGraph resourceGraph)
            : base(resourceGraph)
        {
        }

        public override IDictionary<string, object> GetMeta(TextLanguage resource)
        {
            return new Dictionary<string, object>
            {
                ["Notice"] = NoticeText
            };
        }
    }
}
