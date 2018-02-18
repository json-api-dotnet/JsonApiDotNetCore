using System;
using System.Collections.Generic;
using System.Text;

namespace JsonApiDotNetCore.Configuration
{
    public struct NullAttributeResponseBehavior
    {
        public NullAttributeResponseBehavior(bool omitNullValuedAttributes = false, bool allowClientOverride = false)
        {
            OmitNullValuedAttributes = omitNullValuedAttributes;
            AllowClientOverride = allowClientOverride;
        }

        public bool OmitNullValuedAttributes { get; }
        public bool AllowClientOverride { get; }
        // ...
    }
}
