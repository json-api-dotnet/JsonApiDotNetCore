using System;
using System.Diagnostics;
using System.Linq;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization;

namespace JsonApiDotNetCoreExample.Models
{
    public sealed class ThrowingResource : Identifiable
    {
        [Attr]
        public string FailsOnSerialize
        {
            get
            {
                var isSerializingResponse = new StackTrace().GetFrames()
                    .Any(frame => frame.GetMethod().DeclaringType == typeof(JsonApiWriter));
                
                if (isSerializingResponse)
                {
                    throw new InvalidOperationException($"The value for the '{nameof(FailsOnSerialize)}' property is currently unavailable.");
                }

                return string.Empty;
            }
            set { }
        }
    }
}
