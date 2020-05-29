using System;
using System.Diagnostics;
using System.Linq;
using JsonApiDotNetCore.Formatters;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;

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
